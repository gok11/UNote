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
    [InitializeOnLoad]
    public static class EditorUNoteManager
    {
        #region Field

        private static NoteType s_currentNoteType = NoteType.Project;
        private static NoteBase s_currentNote;

        private static ProjectNoteContainer s_projectNoteContainer;
        private static ProjectNoteIdConvertData s_projectNoteIdConvertData;

        #endregion // Field

        #region Property

        public static NoteType CurrentNoteType => s_currentNoteType;
        public static NoteBase CurrentNote
        {
            get
            {
                if (s_currentNote == null)
                {
                    switch (s_currentNoteType)
                    {
                        case NoteType.Project:
                            s_currentNote = GetAllRootProjectNoteList()?.FirstOrDefault();
                            break;
                    }
                }

                return s_currentNote;
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
        public static void InitializeEditorNote()
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;

            s_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
            s_projectNoteContainer.Load();

            s_projectNoteIdConvertData =
                ScriptableObject.CreateInstance<ProjectNoteIdConvertData>();
            s_projectNoteIdConvertData.Load(authorName);
        }

        public static void Select(NoteBase note)
        {
            s_currentNote = note;
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

        public static ProjectNote AddNewRootProjectNote()
        {
            Guid guid = Guid.NewGuid();

            s_projectNoteIdConvertData.SetTitle(guid.ToString(), "New Note");
            ProjectNoteIDManager.ResetData();

            ProjectNote newNote = new ProjectNote(guid.ToString());
            newNote.IsRootNote = true;

            Undo.RecordObject(s_projectNoteContainer, "UNote Add New Project Note");
            s_projectNoteContainer.GetOwnList().Add(newNote);

            s_projectNoteContainer.Save();
            return newNote;
        }

        public static ProjectNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            ProjectNote newNote = new ProjectNote(guid);
            newNote.IsRootNote = false;
            newNote.NoteContent = noteContent;

            Undo.RecordObject(s_projectNoteContainer, "UNote Add New Project Note");
            s_projectNoteContainer.GetOwnList().Add(newNote);

            s_projectNoteContainer.Save();
            return newNote;
        }

        public static IEnumerable<IReadOnlyList<ProjectNote>> GetAllProjectNotes()
        {
            return s_projectNoteContainer.GetListAll();
        }

        public static IReadOnlyList<ProjectNote> GetAllRootProjectNoteList()
        {
            return GetAllProjectNotes().SelectMany(t => t).Where(t => t.IsRootNote).ToList();
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
                    ProjectNote projectNote = note as ProjectNote;
                    List<ProjectNote> projectList = s_projectNoteContainer.GetOwnList();
                    if (projectList.Contains(projectNote))
                    {
                        projectList.Remove(projectNote);
                        s_projectNoteContainer.Save();

                        // Rootなら変換情報も削除
                        if (projectNote.IsRootNote)
                        {
                            s_projectNoteIdConvertData.DeleteTable(projectNote.ProjectNoteID);
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
    }
}
