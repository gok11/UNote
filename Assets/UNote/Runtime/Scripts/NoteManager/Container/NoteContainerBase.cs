using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class NoteContainerBase : ScriptableObject
    {
        #region Property
        
        protected abstract string Identifier { get; }

        protected abstract string SubDirectoryName { get; }

        protected virtual string FileDirectory
        {
            get
            {
                string streamingAssets = Application.streamingAssetsPath;
                return Path.Combine(streamingAssets, "UNote", SubDirectoryName);
            }
        }

        protected virtual string OwnFileName => $"{UNoteSetting.UserName}_{Identifier}.asset";

        protected virtual string OwnFileFullPath => Path.Combine(FileDirectory, OwnFileName);

        #endregion // Property

        public abstract void Save();
    }
}
