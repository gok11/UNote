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
    public class ProjectNoteIdConvertData
    {
        public List<ProjectNoteIdConvertTable> convertTableList = new();

        public void SetTitle(string guid, string newTitle)
        {
            convertTableList.Add(
                new ProjectNoteIdConvertTable() { guid = guid, title = newTitle, }
            );

            Save();
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

            string json = JsonUtility.ToJson(this);

            File.WriteAllText(filePath, json);
        }
    }
}
