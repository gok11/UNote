using System;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectNoteComment : NoteCommentBase
    {
        #region Property

        public override NoteType NoteType => NoteType.Project;

        #endregion // Property
    }
}
