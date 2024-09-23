using System;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectNote : RootNoteBase
    {
        #region Property
        public override NoteType NoteType => NoteType.Project;

        #endregion // Property
    }
}
