using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using UNote.Runtime;

using Object = UnityEngine.Object;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentNote;

        private ProjectNoteContainer m_projectNoteContainer;
        private AssetNoteContainer m_assetNoteContainer;

        public delegate void AddNoteHandler(NoteBase note);

        public delegate void SelectNoteHandler(NoteBase note);
        public delegate void DeleteNoteHandler(NoteBase note);

        public static event AddNoteHandler OnNoteAdded;
        public static event SelectNoteHandler OnNoteSelected;
        public static event DeleteNoteHandler OnNoteDeleted;

        private static EditorUNoteManager s_instance;

        #endregion // Field

        #region Property

        public static EditorUNoteManager Instance => s_instance ??= new EditorUNoteManager();

        public static NoteType CurrentNoteType => Instance.m_currentNoteType;

        public static NoteBase CurrentNote
        {
            get
            {
                if (Instance.m_currentNote != null)
                {
                    return Instance.m_currentNote;
                }

                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        Instance.m_currentNote = GetAllProjectNotes()?.FirstOrDefault();
                        break;
                    
                    case NoteType.Asset:
                        Instance.m_currentNote = GetAllAssetLeafNotes()?.FirstOrDefault();
                        break;
                }

                return Instance.m_currentNote;
            }
        }

        private static ProjectNoteContainer ProjectNoteContainer
        {
            get
            {
                if (Instance.m_projectNoteContainer)
                {
                    return Instance.m_projectNoteContainer;
                }
                
                Instance.m_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
                Instance.m_projectNoteContainer.Load();
                return Instance.m_projectNoteContainer;
            }
        }

        private static AssetNoteContainer AssetNoteContainer
        {
            get
            {
                if (Instance.m_assetNoteContainer)
                {
                    return Instance.m_assetNoteContainer;
                }
                
                Instance.m_assetNoteContainer = ScriptableObject.CreateInstance<AssetNoteContainer>();
                Instance.m_assetNoteContainer.Load();
                return Instance.m_assetNoteContainer;
            }
        }
        
        #endregion // Property

        #region Public Method

        public static void SelectCategory(NoteType noteType)
        {
            Instance.m_currentNote = null;
            Instance.m_currentNoteType = noteType;
            OnNoteSelected?.Invoke(null);
        }

        public static void SelectNote(NoteBase note)
        {
            Instance.m_currentNote = note;
            Instance.m_currentNoteType = note.NoteType;
            OnNoteSelected?.Invoke(note);
        }

        #region Project Note
        
        public static SerializedObject GetProjectNoteContainerObject()
        {
            return new SerializedObject(ProjectNoteContainer);
        }

        public static ProjectNote AddNewProjectNote()
        {
            Undo.RecordObject(ProjectNoteContainer, "UNote Add New Project Note");
            
            ProjectNote newNote = new ProjectNote
            {
                Author = UNoteSetting.UserName
            };

            string uniqueName = GenerateUniqueName(NoteType.Project);
            newNote.ChangeNoteName(uniqueName);
            
            ProjectNoteContainer.GetOwnProjectNoteList().Add(newNote);
            ProjectNoteContainer.Save();
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        public static ProjectLeafNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            Undo.RegisterCompleteObjectUndo(ProjectNoteContainer, "UNote Add New Project Note");
            
            ProjectLeafNote newNote = new ProjectLeafNote
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                NoteId = guid
            };

            ProjectNoteContainer.GetOwnProjectLeafNoteList().Add(newNote);
            ProjectNoteContainer.Save();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        public static SerializedObject CreateProjectNoteContainerObject()
        {
            return new SerializedObject(ProjectNoteContainer);
        }

        #endregion // Project Note

        #region Asset Note

        public static SerializedObject GetAssetNoteContainerObject()
        {
            return new SerializedObject(AssetNoteContainer);
        }
        
        public static AssetNote AddNewAssetNote(string guid, string noteContent)
        {
            Undo.RegisterCompleteObjectUndo(AssetNoteContainer, "UNote Add New Asset Note");
            
            AssetNote newNote = new AssetNote
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                NoteId = guid
            };

            AssetNoteContainer.GetOwnAssetLeafNoteList().Add(newNote);
            AssetNoteContainer.Save();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        public static IEnumerable<AssetNote> GetAllAssetLeafNotes() =>
            AssetNoteContainer.GetAssetLeafNoteListAll().SelectMany(t => t);

        public static IEnumerable<AssetNote> GetAllAssetLeafNotesDistinct() =>
            GetAllAssetLeafNotes().GroupBy(t => t.NoteId)
                .Select(t => t.OrderBy(t2 => t2.CreatedDate).First());
        
        public static IReadOnlyList<AssetNote> GetAssetLeafNoteListByNoteId(string noteId) =>
            AssetNoteContainer.GetAssetLeafNoteListByGuid(noteId);

        public static SerializedObject CreateAssetNoteContainerObject()
        {
            return new SerializedObject(AssetNoteContainer);
        }
        
        #endregion // Asset Note
        
        public static void ChangeNoteName(NoteBase note, string noteName)
        {
            switch (note?.NoteType)
            {
                case NoteType.Project:
                    if (note is ProjectNote projectNote)
                    {
                        Undo.RecordObject(ProjectNoteContainer, "UNote Change Project Note Title");
                        projectNote.ChangeNoteName(noteName);
                        ProjectNoteContainer.Save();
                    }
                    break;
            }
        }

        public static void DeleteNote(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                    Undo.RecordObject(ProjectNoteContainer, "Delete Project Note");
                    
                    if (note is ProjectNote projectNote)
                    {
                        List<ProjectNote> projectList =
                            ProjectNoteContainer.GetOwnProjectNoteList();
                        if (projectList.Contains(projectNote))
                        {
                            projectList.Remove(projectNote);
                            ProjectNoteContainer.Save();
                        }
                    }
                    else if (note is ProjectLeafNote projectLeafNote)
                    {
                        List<ProjectLeafNote> projectList =
                            ProjectNoteContainer.GetOwnProjectLeafNoteList();
                        if (projectList.Contains(projectLeafNote))
                        {
                            projectList.Remove(projectLeafNote);
                            ProjectNoteContainer.Save();
                        }
                    }
                    break;
                
                case NoteType.Asset:
                    Undo.RecordObject(AssetNoteContainer, "Delete Asset Note");
                    
                    if (note is AssetNote assetLeafNote)
                    {
                        List<AssetNote> assetLeafNoteList =
                            AssetNoteContainer.GetOwnAssetLeafNoteList();
                        if (assetLeafNoteList.Contains(assetLeafNote))
                        {
                            assetLeafNoteList.Remove(assetLeafNote);
                            AssetNoteContainer.Save();
                        }
                    }
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            if (note is RootNoteBase)
            {
                Instance.m_currentNote = null;   
            }
            OnNoteDeleted?.Invoke(note);
        }

        public static void ToggleArchived(NoteBase note)
        {
            note.Archived = !note.Archived;

            SaveAll();
        }

        public static void SaveAll()
        {
            ProjectNoteContainer.Save();
            AssetNoteContainer.Save();
        }
        #endregion // Public Method

        #region Private Method

        private static string GenerateUniqueName(NoteType noteType)
        {
            const string baseName = "New Note";

            switch (noteType)
            {
                case NoteType.Project:
                    string noteName = baseName;
                    int id = 0;
                    var notes = GetAllProjectNotes();
                    while (true)
                    {
                        bool isOverlap = false;
                        foreach (var note in notes)
                        {
                            if (note.NoteName == noteName)
                            {
                                isOverlap = true;
                                break;
                            }
                        }

                        if (!isOverlap)
                        {
                            break;
                        }

                        noteName = $"{baseName} {id++}";
                    }
                    return noteName;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion // Private Method
    }
}
