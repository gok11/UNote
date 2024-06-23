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
    public class EditorUNoteManager
    {
        #region Field

        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentRootNote;
        private NoteBase m_currentLeafNote;

        private ProjectNoteContainer m_projectNoteContainer;
        private AssetNoteContainer m_assetNoteContainer;

        private static EditorUNoteManager s_instance;

        #endregion // Field

        #region Property

        public static EditorUNoteManager Instance => s_instance ??= new EditorUNoteManager();

        public static NoteType CurrentNoteType => Instance.m_currentNoteType;

        public static NoteBase CurrentRootNote
        {
            get
            {
                if (Instance.m_currentRootNote != null)
                {
                    return Instance.m_currentRootNote;
                }

                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        Instance.m_currentRootNote = GetAllProjectNotes()?.FirstOrDefault();
                        break;
                    
                    case NoteType.Asset:
                        Instance.m_currentRootNote = GetAllAssetNotes()?.FirstOrDefault();
                        break;
                }

                return Instance.m_currentRootNote;
            }
        }

        public static NoteBase CurrentLeafNote
        {
            get
            {
                if (Instance.m_currentLeafNote != null)
                {
                    return Instance.m_currentLeafNote;
                }

                string noteId = CurrentRootNote.NoteId;
                
                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        Instance.m_currentLeafNote =
                            GetProjectLeafNoteListByProjectNoteId(noteId).FirstOrDefault();
                        break;
                    
                    case NoteType.Asset:
                        Instance.m_currentLeafNote =
                            GetProjectLeafNoteListByProjectNoteId(noteId).FirstOrDefault();
                        break;
                }

                return Instance.m_currentLeafNote;
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
            Instance.m_currentRootNote = null;
            Instance.m_currentLeafNote = null;

            Instance.m_currentNoteType = noteType;
        }

        public static void SelectRoot(NoteBase note)
        {
            Instance.m_currentRootNote = note;
            Instance.m_currentLeafNote = null;

            Instance.m_currentNoteType = note.NoteType;
        }

        public static void SelectLeaf(NoteBase note)
        {
            Instance.m_currentLeafNote = note;
            Instance.m_currentNoteType = note.NoteType;

            string noteId = note.NoteId;

            // Root を探して設定
            switch (note.NoteType)
            {
                case NoteType.Project:
                    var projectNotes = GetAllProjectNotes();
                    foreach (var projectNote in projectNotes)
                    {
                        if (projectNote.NoteId == noteId)
                        {
                            Instance.m_currentRootNote = projectNote;
                            break;
                        }
                    }
                    break;
                
                case NoteType.Asset:
                    var assetNotes = GetAllAssetNotes();
                    foreach (var assetNote in assetNotes)
                    {
                        if (assetNote.NoteId == noteId)
                        {
                            Instance.m_currentRootNote = assetNote;
                            break;
                        }
                    }
                    break;
                
                default:
                    throw new NotImplementedException();
            }
        }

        #region Project Note

        public static ProjectNote AddNewProjectNote()
        {
            Undo.RecordObject(ProjectNoteContainer, "UNote Add New Project Note");
            
            ProjectNote newNote = new ProjectNote
            {
                Author = UserConfig.GetUNoteSetting().UserName
            };

            string uniqueName = GenerateUniqueName(NoteType.Project);
            newNote.ChangeNoteName(uniqueName);

            ProjectNoteContainer.GetOwnProjectNoteList().Add(newNote);
            ProjectNoteContainer.Save();
            
            return newNote;
        }

        public static ProjectLeafNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            Undo.RecordObject(ProjectNoteContainer, "UNote Add New Project Note");
            
            ProjectLeafNote newNote = new ProjectLeafNote
            {
                Author = UserConfig.GetUNoteSetting().UserName,
                NoteContent = noteContent,
                NoteId = guid
            };

            ProjectNoteContainer.GetOwnProjectLeafNoteList().Add(newNote);
            ProjectNoteContainer.Save();
            
            return newNote;
        }

        public static IEnumerable<ProjectNote> GetAllProjectNotes() =>
            ProjectNoteContainer.GetProjectNoteListAll().SelectMany(t => t);

        public static IEnumerable<ProjectLeafNote> GetAllProjectLeafNotes() =>
            ProjectNoteContainer.GetProjectLeafNoteListAll().SelectMany(t => t);
        

        public static IReadOnlyList<ProjectLeafNote> GetProjectLeafNoteListByProjectNoteId(string noteId) =>
            ProjectNoteContainer.GetProjectLeafNoteListByProjectNoteId(noteId);

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

        public static AssetNote AddNewAssetNote(Object asset)
        {
            if (!asset)
            {
                Debug.LogError("Asset is null");
                return null;
            }
            
            Undo.RecordObject(AssetNoteContainer, "UNote Add New Asset Note");

            string assetPath = AssetDatabase.GetAssetPath(asset);
            
            AssetNote newNote = new AssetNote
            {
                Author = UserConfig.GetUNoteSetting().UserName,
                NoteId = AssetDatabase.AssetPathToGUID(assetPath),
            };

            newNote.ChangeNoteName(asset.name);

            AssetNoteContainer.GetOwnAssetNoteList().Add(newNote);
            AssetNoteContainer.Save();
            
            return newNote;
        }
        
        public static AssetLeafNote AddNewLeafAssetNote(string guid, string noteContent)
        {
            Undo.RecordObject(AssetNoteContainer, "UNote Add New Project Note");
            
            AssetLeafNote newNote = new AssetLeafNote
            {
                Author = UserConfig.GetUNoteSetting().UserName,
                NoteContent = noteContent,
                NoteId = guid
            };

            AssetNoteContainer.GetOwnAssetLeafNoteList().Add(newNote);
            AssetNoteContainer.Save();
            
            return newNote;
        }

        public static IEnumerable<AssetNote> GetAllAssetNotes() =>
            AssetNoteContainer.GetAssetNoteListAll().SelectMany(t => t);

        public static IEnumerable<AssetLeafNote> GetAllAssetLeafNotes() =>
            AssetNoteContainer.GetAssetLeafNoteListAll().SelectMany(t => t);

        public static IReadOnlyList<AssetLeafNote> GetAssetLeafNoteListByNoteId(string noteId) =>
            AssetNoteContainer.GetProjectLeafNoteListByNoteId(noteId);

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

                            // TODO 他の所属Leaf削除
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
                
                default:
                    throw new NotImplementedException();
            }
        }

        public static void ToggleArchived(NoteBase note)
        {
            note.Archived = !note.Archived;

            SaveAll();
        }

        public static void SaveAll()
        {
            ProjectNoteContainer.Save();
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
