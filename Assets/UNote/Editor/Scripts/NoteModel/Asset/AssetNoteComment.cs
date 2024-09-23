using System;
using UNote.Runtime;

namespace UNote.Editor
{
    [Serializable]
    public sealed class AssetNoteComment : NoteCommentBase
    {
        #region Property

        public override NoteType NoteType => NoteType.Asset;

        #endregion // Property
    }
}
