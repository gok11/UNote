using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class AssetLeafNote : LeafNoteBase
    {
        #region Property

        public override NoteType NoteType => NoteType.Asset;

        #endregion
    }
}
