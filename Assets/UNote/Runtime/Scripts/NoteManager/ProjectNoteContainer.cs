using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : ScriptableObject
    {
        #region  Const

        private const string TypeSuffix = "project";

        #endregion // Const

        #region Field

        [SerializeField]
        private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();

        #endregion // Field

        #region Property

        private string FileDirectory
        {
            get
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                return Path.Combine(projectRoot, "UNote");
            }
        }

        private string FileName
        {
            get { return $"{UserConfig.GetUNoteSetting().UserName}_{TypeSuffix}.json"; }
        }

        private string FileFullPath
        {
            get { return Path.Combine(FileDirectory, FileName); }
        }

        public List<ProjectNote> ProjectNoteList => m_projectNoteList;

        #endregion // Property

        public void Save()
        {
            if (!Directory.Exists(FileDirectory))
            {
                Directory.CreateDirectory(FileDirectory);
            }

            // Serialize
            string json = JsonUtility.ToJson(m_projectNoteList);
            File.WriteAllText(FileFullPath, json);
        }

        public void Load()
        {
            if (File.Exists(FileFullPath))
            {
                string json = File.ReadAllText(FileFullPath);

                // Deserialize
                m_projectNoteList = JsonUtility.FromJson<List<ProjectNote>>(json);
            }
            else
            {
                m_projectNoteList = new List<ProjectNote>();
            }
        }
    }
}
