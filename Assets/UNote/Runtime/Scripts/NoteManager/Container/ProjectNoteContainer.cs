using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region  Const

        protected override string Identifier => "project";

        protected override string SubDirectoryName => "Project";

        #endregion // Const

        #region Field

        [SerializeField]
        private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();

        #endregion // Field

        #region Property

        public List<ProjectNote> ProjectNoteList
        {
            get
            {
                if (m_projectNoteList == null)
                {
                    Load();
                }

                return m_projectNoteList;
            }
        }

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
                Save();
            }
        }
    }
}
