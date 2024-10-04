using System;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectNote : RootNoteBase
    {
        public override NoteType NoteType => NoteType.Project;
    }
}
