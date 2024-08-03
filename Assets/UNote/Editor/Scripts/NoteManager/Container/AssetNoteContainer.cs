using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class AssetNoteContainer : NoteContainerBase
    {
        #region Field

        [SerializeField] private List<AssetNote> m_assetNoteList;
        [SerializeField] private List<AssetLeafNote> m_assetLeafNoteList;
        
        #endregion // Field

        public List<AssetNote> GetAssetNoteList() => m_assetNoteList;
        public List<AssetLeafNote> GetAssetLeafNoteList() => m_assetLeafNoteList;
    }
}
