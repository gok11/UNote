using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class EditorUNoteManager
    {
        #region Field

        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentRootNote;
        private NoteBase m_currentLeafNote;

        private ProjectNoteContainer m_projectNoteContainer;

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

                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        string noteId = CurrentRootNote.NoteId;
                        Instance.m_currentLeafNote = GetAllProjectLeafNotes()
                            .FirstOrDefault(t => t.NoteId == noteId);
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

            // Root を探して設定
            switch (note.NoteType)
            {
                case NoteType.Project:
                    string projectNoteId = note.NoteId;
                    var projectNotes = GetAllProjectNotes();
                    foreach (var projectNote in projectNotes)
                    {
                        if (projectNote.NoteId == projectNoteId)
                        {
                            Instance.m_currentRootNote = projectNote;
                            break;
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        #region Project Note

        public static SerializedObject GetProjectNoteContainerObject()
        {
            return new SerializedObject(ProjectNoteContainer);
        }

        public static ProjectNote AddNewProjectNote()
        {
            Undo.RecordObject(ProjectNoteContainer, "UNote Add New Project Note");
            
            ProjectNote newNote = new ProjectNote();
            newNote.Author = UserConfig.GetUNoteSetting().UserName;

            string uniqueName = GenerateUniqueName(NoteType.Project);
            newNote.ChangeProjectNoteName(uniqueName);

            ProjectNoteContainer.GetOwnProjectNoteList().Add(newNote);
            ProjectNoteContainer.Save();
            
            return newNote;
        }

        public static ProjectLeafNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            Undo.RecordObject(ProjectNoteContainer, "UNote Add New Project Note");
            
            ProjectLeafNote newNote = new ProjectLeafNote();
            newNote.Author = UserConfig.GetUNoteSetting().UserName;
            newNote.NoteContent = noteContent;
            newNote.NoteId = guid;
            
            ProjectNoteContainer.GetOwnProjectLeafNoteList().Add(newNote);
            ProjectNoteContainer.Save();
            
            return newNote;
        }

        public static IEnumerable<ProjectNote> GetAllProjectNotes()
        {
            return ProjectNoteContainer.GetProjectNoteListAll().SelectMany(t => t);
        }

        public static IEnumerable<ProjectLeafNote> GetAllProjectLeafNotes()
        {
            return ProjectNoteContainer.GetProjectLeafNoteListAll().SelectMany(t => t);
        }

        public static SerializedObject CreateProjectNoteContainerObject()
        {
            return new SerializedObject(ProjectNoteContainer);
        }

        #endregion // Project Note

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

                            // TODO 最後の一つなら変換テーブル削除
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
