using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public class UserConfig
    {
        public const string SettingKey = "UNote.UserConfig";

        public static UNoteSetting GetSetting()
        {
            UNoteSetting setting = Resources.Load<UNoteSetting>("UNoteSetting");

#if UNITY_EDITOR
            if (!setting)
            {
                UNoteSetting newSetting = ScriptableObject.CreateInstance<UNoteSetting>();
                string runtimeFolderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(
                    "088cd7db0bd4c044e988668cfbd71988"
                );
                string resourcesFolderPath = $"{runtimeFolderPath}/Resources/";
                if (!Directory.Exists(resourcesFolderPath))
                {
                    Directory.CreateDirectory(resourcesFolderPath);
                }
                UnityEditor.AssetDatabase.CreateAsset(
                    newSetting,
                    $"{resourcesFolderPath}/UNoteSetting.asset"
                );
            }
#endif

            return setting;
        }
    }

    public class UNoteSetting : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private string m_userName;

        public string UserName
        {
            get => m_userName;
#if UNITY_EDITOR
            set
            {
                if (m_userName != value)
                {
                    m_userName = value;
                }
            }
#endif
        }
    }
}
