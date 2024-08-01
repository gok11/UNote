using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class NoteContainerBase : ScriptableObject
    {
        #region Property

        protected static string FileDirectory
        {
            get
            {
                string streamingAssets = Application.streamingAssetsPath;
                return Path.Combine(streamingAssets, "UNote");
            }
        }
        
        #endregion // Property

        public virtual void Save()
        {
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
