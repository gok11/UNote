using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UNote.Runtime
{
    public static class ProjectNoteIDConverter
    {
        private static Dictionary<string, string> s_convertDict = null;

        public static string Convert(string guid)
        {
            if (s_convertDict == null)
            {
                Initialize();
            }

            if (!s_convertDict.ContainsKey(guid))
            {
                return null;
            }

            return s_convertDict[guid];
        }

        private static void Initialize()
        {
            // StreamingAssets/UNote/ProjectNoteIdConvert にある json から ID 変換する
            string convertDir = Path.Combine(
                Application.streamingAssetsPath,
                "UNote",
                "ProjectNoteIdConvert"
            );
            string[] convertFiles = Directory.GetFiles(convertDir, "*.txt");

            foreach (var convertFile in convertFiles)
            {
                using (StreamReader reader = new StreamReader(convertFile))
                {
                    string text = reader.ReadToEnd();
                    ProjectNoteIdConvertData data = JsonUtility.FromJson<ProjectNoteIdConvertData>(
                        text
                    );

                    foreach (var table in data.convertTableList)
                    {
                        s_convertDict.Add(table.guid, table.title);
                    }
                }
            }
        }

        public static void ResetData()
        {
            s_convertDict = null;
        }
    }
}
