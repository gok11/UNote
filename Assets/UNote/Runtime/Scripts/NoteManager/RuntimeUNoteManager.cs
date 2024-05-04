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

        public static void AddProjectNote()
        {
            ProjectNote newNote = new ProjectNote();
            s_projectNoteContainer.GetList(UserConfig.GetUNoteSetting().UserName).Add(newNote);
        }

        public static IReadOnlyList<ProjectNote> GetProjectNoteList()
        {
            return s_projectNoteContainer.GetList(UserConfig.GetUNoteSetting().UserName);
        }

#if UNITY_EDITOR
        public static SerializedObject CreateProjectNoteContainerObject()
        {
            return new SerializedObject(s_projectNoteContainer);
        }
#endif

        #endregion // Project Note

        public static void SaveAll()
        {
            s_projectNoteContainer.Save();
        }

        #endregion // Public Method
    }
}
