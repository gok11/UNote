using System.Collections.Generic;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class AssetNoteContainer : NoteContainerBase
    {
        #region Field

        [SerializeField] private List<AssetNote> m_assetNoteList = new();
        [SerializeField] private List<AssetLeafNote> m_assetLeafNoteList = new();
        
        #endregion // Field

        public List<AssetNote> GetAssetNoteList() => m_assetNoteList;
        public List<AssetLeafNote> GetAssetLeafNoteList() => m_assetLeafNoteList;
    }
}
