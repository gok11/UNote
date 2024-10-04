using System;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectNoteMessage : NoteMessageBase
    {
        public override NoteType NoteType => NoteType.Project;
    }
}
