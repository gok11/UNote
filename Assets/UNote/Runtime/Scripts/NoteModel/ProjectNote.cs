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
        #endregion // Property

        #region Constructor
        public ProjectNote(string editor)
            : base(editor) { }
        #endregion // Constructor
    }
}
