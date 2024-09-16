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
        [SerializeField] private string m_queryId;
        [SerializeField] private string m_queryName;
        [SerializeField] private NoteTypeFilter m_noteTypeFilter;
        [SerializeField] private string m_searchText;
        [SerializeField] private string[] m_searchTags;
        [SerializeField] private bool m_showArchive;
        [SerializeField] private bool m_showFavoriteFirst;

        public NoteQuery(string queryName, string searchText,
            string[] searchTags, bool showArchive, bool showFavoriteFirst)
        {
            QueryName = queryName;
            NoteTypeFilter = NoteTypeFilter.All;
            QueryId = Guid.NewGuid().ToString();
        }

        #region Property

        public string QueryId
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

        public bool ShowArchive
        {
            get => m_showArchive;
            set => m_showArchive = value;
        }

        public bool ShowFavoriteFirst
        {
            get => m_showFavoriteFirst;
            set => m_showFavoriteFirst = value;
        }

        internal virtual bool IsOverWritable => true;

        #endregion
    }
}
