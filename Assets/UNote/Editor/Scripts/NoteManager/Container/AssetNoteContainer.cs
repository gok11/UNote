using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UNote.Runtime;

namespace UNote.Editor
{
    public class AssetNoteContainer : NoteContainerBase
    {
        #region Field

        [SerializeField] private List<AssetNote> m_assetNoteList = new();
        [FormerlySerializedAs("m_assetLeafNoteList")] [SerializeField] private List<AssetNoteComment> m_assetNoteCommentList = new();
        
        #endregion // Field

        public List<AssetNote> GetAssetNoteList() => m_assetNoteList;
        public List<AssetNoteComment> GetAssetNoteCommentList() => m_assetNoteCommentList;
    }
}
