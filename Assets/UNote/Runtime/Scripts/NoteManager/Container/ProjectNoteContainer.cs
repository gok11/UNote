using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region Define

        [Serializable]
        private class ProjectInternalContainer
        {
            public List<ProjectNote> m_projectNoteList = new List<ProjectNote>();
            public List<ProjectLeafNote> m_projectLeafNoteList = new List<ProjectLeafNote>();
        }

        #endregion // Define

        #region Field

        [SerializeField] private ProjectInternalContainer m_projectNoteContainer;
        private Dictionary<string, ProjectInternalContainer> m_projectNoteDict = new();
        private Dictionary<string, List<ProjectLeafNote>> m_projectNoteDictByTitle = new();

        #endregion // Field
        
        #region  Property

        protected override string Identifier => "project";

        protected override string SubDirectoryName => "Project";

        #endregion // Property

        public override void Load()
        {
            Load<ProjectInternalContainer>();
        }

        protected override void LoadData<T>(T container, string authorName)
        {
            if (container is ProjectInternalContainer projectContainer)
            {
                m_projectNoteDict[authorName] = projectContainer;
            }
        }

        public override void Save()
        {
            Save(GetContainerSafe());
        }

        private ProjectInternalContainer GetContainerSafe(string authorName = null)
        {
            authorName ??= UserConfig.GetUNoteSetting().UserName;
            
            if (!m_projectNoteDict.ContainsKey(authorName))
            {
                m_projectNoteContainer = new ProjectInternalContainer();
                m_projectNoteDict.Add(authorName, m_projectNoteContainer);
            }
            return m_projectNoteDict[authorName];
        }

        public List<ProjectNote> GetOwnProjectNoteList() => GetContainerSafe().m_projectNoteList;

        public List<ProjectLeafNote> GetOwnProjectLeafNoteList() => GetContainerSafe().m_projectLeafNoteList;

        public IEnumerable<List<ProjectNote>> GetProjectNoteListAll() =>
            m_projectNoteDict.Values.Where(t => t != null).Select(t => t.m_projectNoteList);

        public IEnumerable<List<ProjectLeafNote>> GetProjectLeafNoteListAll() =>
            m_projectNoteDict.Values.Select(t => t.m_projectLeafNoteList);

        public List<ProjectLeafNote> GetProjectLeafNoteListByProjectNoteId(string projectNoteId)
        {
            if (m_projectNoteDictByTitle.TryGetValue(projectNoteId, out var leafNote))
            {
                return leafNote;
            }

            List<ProjectLeafNote> newList = new(64);

            foreach (var noteList in GetProjectLeafNoteListAll())
            {
                foreach (var note in noteList)
                {
                    if (note.NoteId == projectNoteId)
                    {
                        newList.Add(note);
                    }
                }
            }

            m_projectNoteDictByTitle[projectNoteId] = newList;
            return newList;
        }

        public override void ClearCache()
        {
            m_projectNoteDictByTitle.Clear();
        }
    }
}
