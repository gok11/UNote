using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    [InitializeOnLoad]
    public static class EditorUNoteManager
    {
        #region Field

        private static ProjectNoteContainer s_projectNoteContainer;

        #endregion // Field

        #region Constructor
        static EditorUNoteManager()
        {
            InitializeEditorNote();
        }
        #endregion // Constructor

        #region Public Method
        public static void InitializeEditorNote()
        {
            s_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
            s_projectNoteContainer.Load();
        }

        #region Project Note

        public static ProjectNote AddProjectNote()
        {
            ProjectNote newNote = new ProjectNote();
            s_projectNoteContainer.GetOwnList().Add(newNote);
            s_projectNoteContainer.Save();
            return newNote;
        }

        public static IReadOnlyList<ProjectNote> GetOwnProjectNoteList()
        {
            return s_projectNoteContainer.GetOwnList();
        }

        public static SerializedObject CreateProjectNoteContainerObject()
        {
            return new SerializedObject(s_projectNoteContainer);
        }

        #endregion // Project Note

        public static void DeleteNote(NoteBase note)
        {
            if (note is ProjectNote projectNote)
            {
                List<ProjectNote> projectList = s_projectNoteContainer.GetOwnList();
                if (projectList.Contains(projectNote))
                {
                    projectList.Remove(projectNote);
                    s_projectNoteContainer.Save();
                }
            }
        }

        public static void SaveAll()
        {
            s_projectNoteContainer.Save();
        }
        #endregion // Public Method
    }
}
