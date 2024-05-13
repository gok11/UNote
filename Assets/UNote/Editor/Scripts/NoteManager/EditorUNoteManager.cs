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
                            s_currentNote = GetAllProjectNotes()
                                ?.FirstOrDefault()
                                ?.FirstOrDefault();
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

            Debug.Log(CurrentNote.NoteId);
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

        public static ProjectNote AddNewProjectNote()
        {
            Guid guid = Guid.NewGuid();

            s_projectNoteIdConvertData.SetTitle(guid.ToString(), "New Note");
            ProjectNoteIDConverter.ResetData();

            ProjectNote newNote = new ProjectNote(guid.ToString());
            s_projectNoteContainer.GetOwnList().Add(newNote);
            s_projectNoteContainer.Save();
            return newNote;
        }

        public static IReadOnlyList<ProjectNote> GetOwnProjectNoteList()
        {
            return s_projectNoteContainer.GetOwnList();
        }

        public static IEnumerable<IReadOnlyList<ProjectNote>> GetAllProjectNotes()
        {
            return s_projectNoteContainer.GetListAll();
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
                    }
                    break;
            }
        }

        public static void SaveAll()
        {
            s_projectNoteContainer.Save();
        }
        #endregion // Public Method
    }
}
