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
        [FormerlySerializedAs("m_assetNoteCommentList")] [FormerlySerializedAs("m_assetLeafNoteList")] [SerializeField] private List<AssetNoteMessage> m_assetNoteMessageList = new();
        
        #endregion // Field

        public List<AssetNote> GetAssetNoteList() => m_assetNoteList;
        public List<AssetNoteMessage> GetAssetNoteMessageList() => m_assetNoteMessageList;
    }
}
