using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectNote : NoteBase
    {
        #region Field
        [SerializeField]
        private string title;
        #endregion // Field

        #region  Property
        public override NoteType NoteType => NoteType.Project;

        public string Title
        {
            get => title;
            set => title = value;
        }

        #endregion // Property

        #region Constructor
        public ProjectNote(string editor = null)
            : base(editor) { }
        #endregion // Constructor
    }
}
