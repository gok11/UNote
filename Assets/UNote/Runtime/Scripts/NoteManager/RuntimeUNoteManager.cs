using UnityEditor;

namespace UNote.Runtime
{
    [InitializeOnLoad]
    public static class RuntimeUNoteManager
    {
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
