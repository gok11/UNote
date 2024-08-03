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
    public class AssetNote : RootNoteBase
    {
        #region Field

        [SerializeField] private string m_bindAssetId;

        #endregion
        
        #region Property

        public override NoteType NoteType => NoteType.Asset;

        public override string NoteName
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath(NoteId);
                return Path.GetFileName(path);
            }
        }

        public string BindAssetId
        {
            get => m_bindAssetId;
            set => m_bindAssetId = value;
        }

        #endregion // Property
    }
}
