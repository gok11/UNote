using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : ScriptableObject
    {
        private const string TypeSuffix = "project";

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

        [SerializeField]
        private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();

        public string Serialize()
        {
            return JsonUtility.ToJson(m_projectNoteList);
        }

        public void Deserialize(string json)
        {
            m_projectNoteList = JsonUtility.FromJson<List<ProjectNote>>(json);
        }

        public void Save()
        {
            if (!Directory.Exists(FileDirectory))
            {
                Directory.CreateDirectory(FileDirectory);
            }

            string json = Serialize();
            File.WriteAllText(FileFullPath, json);
        }

        public void Load()
        {
            if (File.Exists(FileFullPath))
            {
                string json = File.ReadAllText(FileFullPath);
                Deserialize(json);
            }
        }
    }
}
