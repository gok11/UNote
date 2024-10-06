using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Asset note
    /// </summary>
    [Serializable]
    public class AssetNote : RootNoteBase
    {
        [SerializeField] private string m_bindAssetId;
        
        public override NoteType NoteType => NoteType.Asset;

        public override string NoteName
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath(BindAssetId);
                return Path.GetFileName(path);
            }
        }

        public string BindAssetId
        {
            get => m_bindAssetId;
            set => m_bindAssetId = value;
        }
    }
}
