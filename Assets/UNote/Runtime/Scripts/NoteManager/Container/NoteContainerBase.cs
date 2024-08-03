using System.Collections;
using System.Collections.Generic;
using System.IO;
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
