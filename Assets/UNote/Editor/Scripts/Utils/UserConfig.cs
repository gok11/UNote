using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public class UserConfig
    {
        public const string SettingKey = "UNote.UserConfig";

        public static UNoteSetting GetSetting()
        {
            string setting = EditorUserSettings.GetConfigValue(SettingKey);
            if (!string.IsNullOrEmpty(setting))
            {
                return JsonUtility.FromJson<UNoteSetting>(setting);
            }

            return new UNoteSetting();
        }
    }

    [Serializable]
    public class UNoteSetting
    {
        [SerializeField]
        private string m_userName;

        public string UserName
        {
            get => m_userName;
            set
            {
                if (m_userName != value)
                {
                    m_userName = value;
                    Update();
                }
            }
        }

        public void Update()
        {
            string json = JsonUtility.ToJson(this);
            EditorUserSettings.SetConfigValue(UserConfig.SettingKey, json);
        }
    }
}
