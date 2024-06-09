using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public class ProjectNoteIdConvertTable
    {
        public string guid;
        public string title;
    }

    [Serializable]
    public class ProjectNoteIdConvertData : ScriptableObject
    {
        #region Define

        [Serializable]
        public class InternalData
        {
            public List<ProjectNoteIdConvertTable> m_convertTableList = new();
        }

        #endregion // Define

        [SerializeField]
        private InternalData m_internalData;

        public void Load(string authorName)
        {
            string fileName = $"{authorName}.json";
            string convertDir = Path.Combine(
                Application.streamingAssetsPath,
                "UNote",
                "ProjectNoteIdConvert"
            );
            Directory.CreateDirectory(convertDir);

            string filePath = Path.Combine(convertDir, fileName);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                m_internalData = JsonUtility.FromJson<InternalData>(json);
                return;
            }

            m_internalData = new InternalData();
            string content = JsonUtility.ToJson(m_internalData);
            File.WriteAllText(filePath, content);
        }

        public void SetTitle(string guid, string newTitle)
        {
            ProjectNoteIdConvertTable table = m_internalData.m_convertTableList.Find(t =>
                t.guid == guid
            );
            
            if (table != null)
            {
                table.title = newTitle;
            }
            else
            {
                m_internalData.m_convertTableList.Add(
                    new ProjectNoteIdConvertTable() { guid = guid, title = newTitle, }
                );
            }

            Save();
        }

        public void DeleteTable(string guid)
        {
            ProjectNoteIdConvertTable table = m_internalData.m_convertTableList.Find(t =>
                t.guid == guid
            );
            if (table != null)
            {
                m_internalData.m_convertTableList.Remove(table);
                Save();
            }
        }

        public void Save()
        {
            string fileName = $"{UserConfig.GetUNoteSetting().UserName}.json";
            string convertDir = Path.Combine(
                Application.streamingAssetsPath,
                "UNote",
                "ProjectNoteIdConvert"
            );
            string filePath = Path.Combine(convertDir, fileName);

            string json = JsonUtility.ToJson(m_internalData);

            File.WriteAllText(filePath, json);
        }
    }
}
