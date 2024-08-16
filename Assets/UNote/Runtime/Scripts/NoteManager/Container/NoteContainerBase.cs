using UnityEditor;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class NoteContainerBase : ScriptableObject
    {
        public virtual void Save()
        {
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
