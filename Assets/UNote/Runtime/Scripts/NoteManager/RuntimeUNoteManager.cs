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

        #region Constructor
        static RuntimeUNoteManager()
        {
            InitializeRuntimeNote();
        }
        #endregion // Constructor

        #region Public Method

        public static void InitializeRuntimeNote() { }

        #endregion // Public Method
    }
}
