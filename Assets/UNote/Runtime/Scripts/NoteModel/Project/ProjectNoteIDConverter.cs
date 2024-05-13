using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UNote.Runtime
{
    public static class ProjectNoteIDManager
    {
        private static Dictionary<string, string> s_convertDict = null;

        public static string[] GetAllGuids()
        {
            if (s_convertDict == null)
            {
                Initialize();
            }

            return s_convertDict.Keys.ToArray();
        }

        public static string ConvertGuid(string guid)
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
            s_convertDict = new Dictionary<string, string>();

            // StreamingAssets/UNote/ProjectNoteIdConvert にある json から ID 変換する
            string convertDir = Path.Combine(
                Application.streamingAssetsPath,
                "UNote",
                "ProjectNoteIdConvert"
            );
            string[] convertFiles = Directory.GetFiles(convertDir, "*.json");

            foreach (var convertFile in convertFiles)
            {
                string json = File.ReadAllText(convertFile);
                ProjectNoteIdConvertData.InternalData internalData =
                    JsonUtility.FromJson<ProjectNoteIdConvertData.InternalData>(json);

                foreach (var table in internalData.m_convertTableList)
                {
                    s_convertDict.Add(table.guid, table.title);
                }
            }
        }

        public static void ResetData()
        {
            s_convertDict = null;
        }
    }
}
