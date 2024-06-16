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

        [SerializeField] private string m_noteName;

        #endregion
        
        #region Property
        public override NoteType NoteType => NoteType.Project;

        public override string NoteName => m_noteName;

        #endregion // Property

        public void ChangeProjectNoteName(string newName)
        {
            m_noteName = newName;
        }
    }
}
