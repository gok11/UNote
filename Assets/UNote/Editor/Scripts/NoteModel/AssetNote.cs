using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    [Serializable]
    public sealed class AssetNote : NoteBase
    {
        #region Field

        [SerializeField]
        private string m_guid;

        #endregion // Field

        #region Property

        public override NoteType NoteType => NoteType.Asset;

        public string GUID => m_guid;

        public override string NoteName => "Asset Note";

        #endregion // Property
    }
}
