using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            File.WriteAllText(FileFullPath, json);
        }

        public void Load()
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;
            InitContainerIfNeeded(authorName);

            if (File.Exists(FileFullPath))
            {
                string json = File.ReadAllText(FileFullPath);

                // Deserialize
                m_projectNoteDict[authorName] = JsonUtility.FromJson<InternalContainer>(json);
            }
            else
            {
                Save();
            }
        }

        private void InitContainerIfNeeded(string authorName)
        {
            if (!m_projectNoteDict.ContainsKey(authorName))
            {
                m_projectNoteDict.Add(authorName, new InternalContainer());
            }
        }

        private InternalContainer GetContainerSafe(string authorName)
        {
            InitContainerIfNeeded(authorName);
            return m_projectNoteDict[authorName];
        }

        public List<ProjectNote> GetList(string authorName)
        {
            return GetContainerSafe(authorName).m_projectNoteList;
        }
    }
}
