using UnityEditor;

namespace UNote.Runtime
{
    [InitializeOnLoad]
    public static class RuntimeUNoteManager
    {
        static RuntimeUNoteManager()
        {
            InitializeRuntimeNote();
        }

        public static void InitializeRuntimeNote() { }
    }
}
