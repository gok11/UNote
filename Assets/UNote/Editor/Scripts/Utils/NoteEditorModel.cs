using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    public partial class NoteEditorModel : ScriptableObject
    {
        private SerializedObject m_modelObject;
        public SerializedObject ModelObject => m_modelObject ??= new SerializedObject(this);
    }
}
