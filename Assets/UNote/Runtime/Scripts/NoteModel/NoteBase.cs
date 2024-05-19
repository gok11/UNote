using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public abstract class NoteBase
    {
        #region Field

        [SerializeField]
        protected string m_noteId;

        [SerializeField]
        protected bool m_isRootNote;

        [SerializeField]
        protected string m_author;

        [SerializeField]
        protected string m_noteContent;

        [SerializeField]
        protected string m_createdDate;

        [SerializeField]
        protected string m_updatedDate;

        #endregion // Field

        #region Property

        public abstract NoteType NoteType { get; }

        public string NoteId
        {
            get => m_noteId;
            set => m_noteId = value;
        }

        public string Author
        {
            get => m_author;
            set => m_author = value;
        }

        public string NoteContent
        {
            get => m_noteContent;
            set => m_noteContent = value;
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

        #endregion // Property

        public NoteBase(string author = null)
        {
            if (author == null)
            {
                author = UserConfig.GetUNoteSetting().UserName;
            }

            m_noteId = Guid.NewGuid().ToString();
            m_author = author;
            m_createdDate = m_updatedDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}
