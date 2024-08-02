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
                        Instance.m_currentNote = GetProjectNoteAllList()?.FirstOrDefault();
                        break;
                    
                    case NoteType.Asset:
                        Instance.m_currentNote = GetAssetNoteAllList()?.FirstOrDefault();
                        break;
                }

                return Instance.m_currentNote;
            }
        }

        // private static ProjectNoteContainer ProjectNoteContainer
        // {
        //     get
        //     {
        //         if (Instance.m_projectNoteContainer)
        //         {
        //             return Instance.m_projectNoteContainer;
        //         }
        //         
        //         Instance.m_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
        //         Instance.m_projectNoteContainer.Load();
        //         return Instance.m_projectNoteContainer;
        //     }
        // }

        // private static AssetNoteContainer AssetNoteContainer
        // {
        //     get
        //     {
        //         if (Instance.m_assetNoteContainer)
        //         {
        //             return Instance.m_assetNoteContainer;
        //         }
        //         
        //         Instance.m_assetNoteContainer = ScriptableObject.CreateInstance<AssetNoteContainer>();
        //         Instance.m_assetNoteContainer.Load();
        //         return Instance.m_assetNoteContainer;
        //     }
        // }
        
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

        public static ProjectNote AddNewProjectNote()
        {
            ProjectNoteContainer container = ProjectNoteContainer.GetContainer();
            
            Undo.RecordObject(container, "UNote Add New Project Note");
            
            ProjectNote newNote = new ProjectNote
            {
                Author = UNoteSetting.UserName
            };

            string uniqueName = GenerateUniqueName(NoteType.Project);
            newNote.ChangeNoteName(uniqueName);
            
            container.GetProjectNoteList().Add(newNote);
            container.Save();

            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        public static ProjectLeafNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            ProjectNoteContainer container = ProjectNoteContainer.GetContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Project Note");
            
            ProjectLeafNote newNote = new ProjectLeafNote
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                NoteId = guid
            };

            container.GetProjectLeafNoteList().Add(newNote);
            container.Save();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        #endregion // Project Note

        #region Asset Note
        
        public static AssetNote AddNewAssetNote(string guid, string noteContent)
        {
            AssetNoteContainer container = AssetNoteContainer.GetContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Asset Note");
            
            AssetNote newNote = new AssetNote
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                NoteId = guid
            };

            container.GetAssetNoteList().Add(newNote);
            container.Save();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }
        
        #endregion // Asset Note
        
        public static void ChangeNoteName(NoteBase note, string noteName)
        {
            switch (note?.NoteType)
            {
                case NoteType.Project:
                    if (note is ProjectNote projectNote)
                    {
                        ProjectNoteContainer container = ProjectNoteContainer.GetContainer();
                        Undo.RecordObject(container, "UNote Change Project Note Title");
                        projectNote.ChangeNoteName(noteName);
                        container.Save();
                    }
                    break;
            }
        }

        public static void DeleteNote(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                    ProjectNoteContainer projContainer = ProjectNoteContainer.GetContainer();
                    Undo.RecordObject(projContainer, "Delete Project Note");
                    
                    if (note is ProjectNote projectNote)
                    {
                        List<ProjectNote> projectList = projContainer.GetProjectNoteList();
                        if (projectList.Contains(projectNote))
                        {
                            projectList.Remove(projectNote);
                            projContainer.Save();
                        }
                    }
                    else if (note is ProjectLeafNote projectLeafNote)
                    {
                        List<ProjectLeafNote> projectList = projContainer.GetProjectLeafNoteList();
                        if (projectList.Contains(projectLeafNote))
                        {
                            projectList.Remove(projectLeafNote);
                            projContainer.Save();
                        }
                    }
                    break;
                
                case NoteType.Asset:
                    AssetNoteContainer astContainer = AssetNoteContainer.GetContainer();
                    Undo.RecordObject(astContainer, "Delete Asset Note");
                    
                    if (note is AssetNote assetNote)
                    {
                        List<AssetNote> assetNoteList = astContainer.GetAssetNoteList();
                        if (assetNoteList.Contains(assetNote))
                        {
                            assetNoteList.Remove(assetNote);
                            astContainer.Save();
                        }
                    }
                    else if (note is AssetLeafNote assetLeafNote)
                    {
                        List<AssetLeafNote> assetLeafNoteList = astContainer.GetAssetLeafNoteList();
                        if (assetLeafNoteList.Contains(assetLeafNote))
                        {
                            assetLeafNoteList.Remove(assetLeafNote);
                            astContainer.Save();
                        }
                    }
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            if (note is RootNoteBase)
            {
                // Reset note selection
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
            ProjectNoteContainer.GetContainer().Save();
            AssetNoteContainer.GetContainer().Save();
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
                    var notes = GetProjectNoteAllList();
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
