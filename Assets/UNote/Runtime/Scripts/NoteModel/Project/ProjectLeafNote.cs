using System;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectLeafNote : LeafNoteBase
    {
        #region Property

        public override NoteType NoteType => NoteType.Project;

        #endregion // Property
    }
}
