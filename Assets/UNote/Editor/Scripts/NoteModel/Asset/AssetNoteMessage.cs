using System;
using UNote.Runtime;

namespace UNote.Editor
{
    [Serializable]
    public sealed class AssetNoteMessage : NoteMessageBase
    {
        public override NoteType NoteType => NoteType.Asset;
    }
}
