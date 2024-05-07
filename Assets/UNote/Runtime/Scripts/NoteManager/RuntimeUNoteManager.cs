using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;

namespace UNote.Runtime
{
    [InitializeOnLoad]
    public static class RuntimeUNoteManager
    {
        // TODO scene note

        private static ProjectNoteContainer s_projectNoteContainer;

        #region Constructor
        static RuntimeUNoteManager()
        {
            InitializeRuntimeNote();
        }
        #endregion // Constructor

        #region Public Method

        public static void InitializeRuntimeNote()
        {
            // ProjectNoteContainer
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

#if UNITY_EDITOR

        public static SerializedObject CreateProjectNoteContainerObject()
        {
            return new SerializedObject(s_projectNoteContainer);
        }
#endif

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
