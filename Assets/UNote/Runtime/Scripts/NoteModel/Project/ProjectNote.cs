using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectNote : NoteBase
    {
        #region  Property
        public override NoteType NoteType => NoteType.Project;

        public override string NoteName => ProjectNoteIDManager.ConvertGuid(NoteId);
        #endregion // Property
    }
}
