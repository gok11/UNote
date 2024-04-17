using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : ScriptableObject
    {
        private const string TypeSuffix = "project";
        private string FileName
        {
            get { return $"{UserConfig.GetUNoteSetting().UserName}_{TypeSuffix}"; }
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
    }
}
