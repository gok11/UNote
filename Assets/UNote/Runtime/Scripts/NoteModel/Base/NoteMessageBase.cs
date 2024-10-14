using System;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public abstract class NoteMessageBase : NoteBase
    {
        [SerializeField] private string m_referenceNoteId;

        [SerializeField] private List<string> noteTagDataIdList;
        
        [SerializeField] protected string m_noteContent;
        
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
        
        public string NoteContent
        {
            get => m_noteContent;
            set => m_noteContent = value;
        }
    }
}
