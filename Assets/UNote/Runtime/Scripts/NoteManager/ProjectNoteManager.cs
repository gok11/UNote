using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer
    {
        private const string TypeSuffix = "project";
        private string FileName
        {
            get { return $"{UserConfig.GetUNoteSetting().UserName}_{TypeSuffix}"; }
        }

        [SerializeField]
        private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();

        // public static string Serialize() {
        //     JsonUtility.ToJson()
        // }

        // public static void Deserialize(string json)
        // {
        //     // 設定ファイルあれば読み込んで ScriptableObject に変換する
        // }
    }
}
