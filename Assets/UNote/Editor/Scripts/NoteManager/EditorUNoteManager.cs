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
    public static class EditorUNoteManager
    {
        #region Field

        private static NoteType s_currentNoteType = NoteType.Project;
        private static NoteBase s_currentRootNote;
        private static NoteBase s_currentLeafNote;

        private static ProjectNoteContainer s_projectNoteContainer;
        private static ProjectNoteIdConvertData s_projectNoteIdConvertData;

        #endregion // Field

        #region Property

        public static NoteType CurrentNoteType => s_currentNoteType;

        public static NoteBase CurrentRootNote
        {
            get
            {
                if (s_currentRootNote != null)
                {
                    return s_currentRootNote;
                }

                switch (s_currentNoteType)
                {
                    case NoteType.Project:
                        s_currentRootNote = GetAllProjectNotes()?.FirstOrDefault();
                        break;
                }

                return s_currentRootNote;
            }
        }

        public static NoteBase CurrentLeafNote
        {
            get
            {
                if (s_currentLeafNote != null)
                {
                    return s_currentLeafNote;
                }

                switch (s_currentNoteType)
                {
                    case NoteType.Project:
                        string noteId = CurrentRootNote.NoteId;
                        s_currentLeafNote = GetAllProjectLeafNotes()
                            .Where(t => t.NoteId == noteId)
                            .FirstOrDefault();
                        break;
                }

                return s_currentLeafNote;
            }
        }

        #endregion // Property

        #region Constructor
        static EditorUNoteManager()
        {
            InitializeEditorNote();
        }
        #endregion // Constructor

        #region Public Method
        [InitializeOnLoadMethod]
        public static void InitializeEditorNote()
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;

            s_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
            s_projectNoteContainer.Load();

            s_projectNoteIdConvertData =
                ScriptableObject.CreateInstance<ProjectNoteIdConvertData>();
            s_projectNoteIdConvertData.Load(authorName);
        }

        public static void SelectRoot(NoteBase note)
        {
            s_currentRootNote = note;
            s_currentLeafNote = null;
        }

        public static void SelectLeaf(NoteBase note)
        {
            s_currentLeafNote = note;

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
                            s_currentRootNote = projectNote;
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
            return new SerializedObject(s_projectNoteContainer);
        }

        public static SerializedObject ProjectNoteIdConvertDataObject()
        {
            return new SerializedObject(s_projectNoteIdConvertData);
        }

        public static ProjectNote AddNewProjectNote()
        {
            ProjectNote newNote = new ProjectNote();
            newNote.Author = UserConfig.GetUNoteSetting().UserName;

            string uniqueName = GenerateUniqueName(NoteType.Project);
            s_projectNoteIdConvertData.SetTitle(newNote.NoteId, uniqueName);
            ProjectNoteIDManager.ResetData();

            if (s_projectNoteContainer)
            {
                Undo.RecordObject(s_projectNoteContainer, "UNote Add New Project Note");
            }

            s_projectNoteContainer.GetOwnProjectNoteList().Add(newNote);
            s_projectNoteContainer.Save();
            return newNote;
        }

        public static ProjectLeafNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            ProjectLeafNote newNote = new ProjectLeafNote();
            newNote.Author = UserConfig.GetUNoteSetting().UserName;
            newNote.NoteContent = noteContent;
            newNote.NoteId = guid;

            if (s_projectNoteContainer)
            {
                Undo.RecordObject(s_projectNoteContainer, "UNote Add New Project Note");
            }
            s_projectNoteContainer.GetOwnProjectLeafNoteList().Add(newNote);

            s_projectNoteContainer.Save();
            return newNote;
        }

        public static IEnumerable<ProjectNote> GetAllProjectNotes()
        {
            return s_projectNoteContainer.GetProjectNoteListAll().SelectMany(t => t);
        }

        public static IEnumerable<ProjectLeafNote> GetAllProjectLeafNotes()
        {
            return s_projectNoteContainer.GetProjectLeafNoteListAll().SelectMany(t => t);
        }

        public static SerializedObject CreateProjectNoteContainerObject()
        {
            return new SerializedObject(s_projectNoteContainer);
        }

        #endregion // Project Note

        public static void DeleteNote(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                    if (note is ProjectNote projectNote)
                    {
                        List<ProjectNote> projectList =
                            s_projectNoteContainer.GetOwnProjectNoteList();
                        if (projectList.Contains(projectNote))
                        {
                            projectList.Remove(projectNote);
                            s_projectNoteContainer.Save();

                            // 変換情報も削除
                            s_projectNoteIdConvertData.DeleteTable(projectNote.NoteId);
                        }
                    }
                    else if (note is ProjectLeafNote projectLeafNote)
                    {
                        List<ProjectLeafNote> projectList =
                            s_projectNoteContainer.GetOwnProjectLeafNoteList();
                        if (projectList.Contains(projectLeafNote))
                        {
                            projectList.Remove(projectLeafNote);
                            s_projectNoteContainer.Save();

                            // 変換情報も削除
                            s_projectNoteIdConvertData.DeleteTable(projectLeafNote.NoteId);
                        }
                    }

                    break;
            }
        }

        public static void SaveAll()
        {
            s_projectNoteContainer.Save();
            s_projectNoteIdConvertData.Save();
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
                            if (ProjectNoteIDManager.ConvertGuid(note.NoteId) == noteName)
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
