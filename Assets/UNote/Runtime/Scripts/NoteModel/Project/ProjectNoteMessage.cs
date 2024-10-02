using System;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectNoteMessage : NoteMessageBase
    {
        #region Property

        public override NoteType NoteType => NoteType.Project;

        #endregion // Property
    }
}
