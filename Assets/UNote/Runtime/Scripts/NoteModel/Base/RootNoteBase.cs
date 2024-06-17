using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class RootNoteBase : NoteBase
    {
        #region Field

        [SerializeField] private string m_noteName;

        #endregion
        
        public override NoteType NoteType => NoteType.Invalid;
        
        public override string NoteName => m_noteName;
        
        public void ChangeNoteName(string newName)
        {
            m_noteName = newName;
        }
    }
}
