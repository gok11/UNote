using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Playables;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region Define

        [Serializable]
        private class InternalContainer
        {
            public string m_authorName;
            public List<ProjectNote> m_projectNoteList = new List<ProjectNote>();
            public List<ProjectLeafNote> m_projectLeafNoteList = new List<ProjectLeafNote>();
        }

        #endregion // Define

        #region  Const

        protected override string Identifier => "project";

        protected override string SubDirectoryName => "Project";

        #endregion // Const

        #region Field

        [SerializeField]
        private InternalContainer m_projectNoteContainer;

        private Dictionary<string, InternalContainer> m_projectNoteDict = new();

        private Dictionary<string, List<ProjectLeafNote>> m_projectNoteDictByTitle = new();

        #endregion // Field

        public void Load()
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;

            // 自身のメモをロード
            if (File.Exists(OwnFileFullPath))
            {
                string json = File.ReadAllText(OwnFileFullPath);

                // Deserialize
                m_projectNoteContainer = JsonUtility.FromJson<InternalContainer>(json);
                m_projectNoteDict[authorName] = m_projectNoteContainer;
            }
            else
            {
                Save();
            }

            // 他のユーザーのメモをロード
            string[] filePaths = Directory.GetFiles(FileDirectory, "*.json");
            foreach (var projectNotePath in filePaths)
            {
                if (projectNotePath == OwnFileFullPath)
                {
                    continue;
                }

                string otherAuthorName = Path.GetFileNameWithoutExtension(projectNotePath);
                string otherJson = File.ReadAllText(projectNotePath);
                if (!m_projectNoteDict.ContainsKey(otherAuthorName))
                {
                    m_projectNoteDict.Add(otherAuthorName, null);
                }
                m_projectNoteDict[otherAuthorName] = JsonUtility.FromJson<InternalContainer>(
                    otherJson
                );
            }
        }

        public void Save()
        {
            if (!Directory.Exists(FileDirectory))
            {
                Directory.CreateDirectory(FileDirectory);
            }

            string authorName = UserConfig.GetUNoteSetting().UserName;

            // Serialize
            string json = JsonUtility.ToJson(GetContainerSafe(authorName));
            File.WriteAllText(OwnFileFullPath, json);

            ClearCache();
        }

        private InternalContainer GetContainerSafe(string authorName)
        {
            if (!m_projectNoteDict.ContainsKey(authorName))
            {
                m_projectNoteContainer = new InternalContainer();
                m_projectNoteContainer.m_authorName = UserConfig.GetUNoteSetting().UserName;

                m_projectNoteDict.Add(authorName, m_projectNoteContainer);
            }
            return m_projectNoteDict[authorName];
        }

        public List<ProjectNote> GetProjectNoteList(string authorName)
        {
            return GetContainerSafe(authorName).m_projectNoteList;
        }

        public List<ProjectLeafNote> GetProjectLeafNoteList(string authorName)
        {
            return GetContainerSafe(authorName).m_projectLeafNoteList;
        }

        public List<ProjectNote> GetOwnProjectNoteList()
        {
            return GetProjectNoteList(UserConfig.GetUNoteSetting().UserName);
        }

        public List<ProjectLeafNote> GetOwnProjectLeafNoteList()
        {
            return GetProjectLeafNoteList(UserConfig.GetUNoteSetting().UserName);
        }

        public IEnumerable<List<ProjectNote>> GetProjectNoteListAll()
        {
            return m_projectNoteDict.Values.Where(t => t != null).Select(t => t.m_projectNoteList);
        }

        public IEnumerable<List<ProjectLeafNote>> GetProjectLeafNoteListAll()
        {
            return m_projectNoteDict.Values.Select(t => t.m_projectLeafNoteList);
        }

        public List<ProjectLeafNote> GetProjectLeafNoteListByProjectNoteId(string projectNoteId)
        {
            if (m_projectNoteDictByTitle.ContainsKey(projectNoteId))
            {
                return m_projectNoteDictByTitle[projectNoteId];
            }

            List<ProjectLeafNote> newList = new(48);

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

        public void ClearCache()
        {
            m_projectNoteDictByTitle.Clear();
        }
    }
}
