using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class NoteContainerBase : ScriptableObject
    {
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

        protected virtual string OwnFileName
        {
            get { return $"{UserConfig.GetUNoteSetting().UserName}_{Identifier}.json"; }
        }

        protected virtual string OwnFileFullPath
        {
            get { return Path.Combine(FileDirectory, OwnFileName); }
        }
    }
}
