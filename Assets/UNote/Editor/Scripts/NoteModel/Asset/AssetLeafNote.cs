using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    [Serializable]
    public class AssetLeafNote : NoteBase
    {
        #region Property

        public override NoteType NoteType => NoteType.Asset;

        #endregion // Property
    }
}
