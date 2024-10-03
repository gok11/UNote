using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField] private NoteTags m_searchTags = NoteTags.All;
        [SerializeField] private bool m_displayArchive;
        [SerializeField] private NoteQuerySort m_noteQuerySort;
        
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

        public NoteTags SearchTags
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

        internal NoteQuery()
        {
            m_queryId = Guid.NewGuid().ToString();
            m_queryName = "Query";
        }

        internal NoteQuery Clone()
        {
            return new NoteQuery()
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
    }
}
