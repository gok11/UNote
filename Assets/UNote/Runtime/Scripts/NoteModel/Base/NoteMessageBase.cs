using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class NoteMessageBase : NoteBase
    {
        #region Field

        [SerializeField] private string m_referenceNoteId;

        [SerializeField] private List<string> noteTagDataIdList;

        #endregion // Field

        #region Property

        public string ReferenceNoteId
        {
            get => m_referenceNoteId;
            set => m_referenceNoteId = value;
        }

        public List<string> NoteTagDataIdList
        {
            get => noteTagDataIdList;
            set => noteTagDataIdList = value;
        }

        #endregion // Property
    }
}
