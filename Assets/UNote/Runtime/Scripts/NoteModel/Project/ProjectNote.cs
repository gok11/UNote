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
        private string m_projectNoteId;
        #endregion // Field

        #region  Property
        public override NoteType NoteType => NoteType.Project;

        public string ProjectNoteID
        {
            get => m_projectNoteId;
            set => m_projectNoteId = value;
        }

        #endregion // Property

        #region Constructor
        public ProjectNote(string noteId, string editor = null)
            : base(editor)
        {
            m_projectNoteId = noteId;
        }
        #endregion // Constructor
    }
}
