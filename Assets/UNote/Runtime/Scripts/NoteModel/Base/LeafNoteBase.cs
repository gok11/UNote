using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class LeafNoteBase : NoteBase
    {
        #region Field

        [SerializeField] private string m_referenceNoteId;

        #endregion // Field

        #region Property

        public string ReferenceNoteId
        {
            get => m_referenceNoteId;
            set => m_referenceNoteId = value;
        }

        #endregion // Property
    }
}
