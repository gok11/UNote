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

            public InternalContainer()
            {
                m_authorName = UserConfig.GetUNoteSetting().UserName;
            }
        }

        #endregion // Define

        #region  Const

        protected override string Identifier => "project";

        protected override string SubDirectoryName => "Project";

        #endregion // Const

        #region Field

        private Dictionary<string, InternalContainer> m_projectNoteDict = new();

        private Dictionary<string, List<ProjectNote>> m_projectNoteDictByTitle = new();

        #endregion // Field

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

        public void Load()
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;
            InitContainerIfNeeded(authorName);

            // 自身のメモをロード
            if (File.Exists(OwnFileFullPath))
            {
                string json = File.ReadAllText(OwnFileFullPath);

                // Deserialize
                m_projectNoteDict[authorName] = JsonUtility.FromJson<InternalContainer>(json);
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

        private InternalContainer GetContainerSafe(string authorName)
        {
            InitContainerIfNeeded(authorName);
            return m_projectNoteDict[authorName];
        }

        private void InitContainerIfNeeded(string authorName)
        {
            if (!m_projectNoteDict.ContainsKey(authorName))
            {
                m_projectNoteDict.Add(authorName, new InternalContainer());
            }
        }

        public List<ProjectNote> GetList(string authorName)
        {
            return GetContainerSafe(authorName).m_projectNoteList;
        }

        public List<ProjectNote> GetOwnList()
        {
            return GetList(UserConfig.GetUNoteSetting().UserName);
        }

        public IEnumerable<List<ProjectNote>> GetListAll()
        {
            return m_projectNoteDict.Values.Select(t => t.m_projectNoteList);
        }

        public List<ProjectNote> GetProjectNoteListByProjectNoteId(string projectNoteId)
        {
            if (m_projectNoteDictByTitle.ContainsKey(projectNoteId))
            {
                return m_projectNoteDictByTitle[projectNoteId];
            }

            List<ProjectNote> newList = new(48);

            foreach (var noteList in GetListAll())
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
