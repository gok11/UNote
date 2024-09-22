using System;
using UnityEngine;

namespace UNote.Editor
{
    public enum NoteTypeFilter
    {
        All,
        Project,
        Asset,
    }
    
    [Serializable]
    public class NoteQuery
    {
        #region Field

        [SerializeField] private string m_queryId;
        [SerializeField] private string m_queryName;
        [SerializeField] private NoteTypeFilter m_noteTypeFilter;
        [SerializeField] private string m_searchText;
        [SerializeField] private string[] m_searchTags;
        [SerializeField] private bool m_displayArchive;
        [SerializeField] private NoteQuerySort m_noteQuerySort;

        #endregion // Field

        #region Property

        public string QueryID
        {
            get => m_queryId;
            set => m_queryId = value;
        }
        
        public string QueryName
        {
            get => m_queryName;
            set => m_queryName = value;
        }

        public NoteTypeFilter NoteTypeFilter
        {
            get => m_noteTypeFilter;
            set => m_noteTypeFilter = value;
        }

        public string SearchText
        {
            get => m_searchText;
            set => m_searchText = value;
        }

        public string[] SearchTags
        {
            get => m_searchTags;
            set => m_searchTags = value;
        }

        public bool DisplayArchive
        {
            get => m_displayArchive;
            set => m_displayArchive = value;
        }

        public NoteQuerySort NoteQuerySort
        {
            get => m_noteQuerySort;
            set => m_noteQuerySort = value;
        }

        internal virtual bool IsOverWritable => true;

        #endregion

        #region Internal Method

        internal T Clone<T>() where T : NoteQuery, new()
        {
            return new T()
            {
                m_queryId = QueryID,
                m_queryName = QueryName,
                m_noteTypeFilter = NoteTypeFilter,
                m_searchText = SearchText,
                m_searchTags = SearchTags,
                m_displayArchive = DisplayArchive,
                m_noteQuerySort = NoteQuerySort,
            };
        }

        #endregion // Internal Method
    }
}
