using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    [Serializable]
    public class AssetLeafNote : NoteBase
    {
        #region Property

        public override NoteType NoteType => NoteType.Asset;

        public override string NoteName
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath(NoteId);
                return Path.GetFileNameWithoutExtension(path);
            }
        }

        #endregion // Property
    }
}
