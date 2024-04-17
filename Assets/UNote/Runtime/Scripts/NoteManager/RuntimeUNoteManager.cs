using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
            s_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
            s_projectNoteContainer.Load();
            s_projectNoteContainer.Save();
        }

        #endregion // Public Method
    }
}
