using System;
using System.Globalization;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public abstract class NoteBase
    {
        [SerializeField]
        protected string m_noteId;

        [SerializeField]
        protected string m_author;

        [SerializeField]
        protected string m_createdDate;

        [SerializeField]
        protected string m_updatedDate;

        [SerializeField]
        protected bool m_archived;

        public abstract NoteType NoteType { get; }

        public string NoteId
        {
            get => m_noteId;
            set => m_noteId = value;
        }

        public virtual string NoteName => "";

        public string Author
        {
            get => m_author;
            set => m_author = value;
        }

        public string CreatedDate
        {
            get => m_createdDate;
        }

        public string UpdatedDate
        {
            get => m_updatedDate;
            set => m_updatedDate = value;
        }

        public bool Archived
        {
            get => m_archived;
            set => m_archived = value;
        }

        protected NoteBase()
        {
            m_noteId = Guid.NewGuid().ToString();
            m_createdDate = m_updatedDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}
