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
            get { return $"{UserConfig.GetSetting().UserName}_{TypeSuffix}"; }
        }

        private List<ProjectNote> projectNoteList = new List<ProjectNote>();

        // public static string Serialize() { }

        // public static void Deserialize()
        // {
        //     // 設定ファイルあれば読み込む
        // }
    }
}
