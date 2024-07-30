using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public class UNoteSetting
    {
        private const string SettingKey = "UNote.UNoteSetting";
        
        [SerializeField]
        private string m_userName;

        [SerializeField]
        private bool m_inspectorFoldoutOpened;
        
        public static string UserName
        {
            get => Load().m_userName;
            set
            {
                var uns = Load();
                uns.m_userName = value;
                Save(uns);
            }
        }

        public static bool InspectorFoldoutOpened
        {
            get => Load().m_inspectorFoldoutOpened;
            set
            {
                var uns = Load();
                uns.m_inspectorFoldoutOpened = value;
                Save(uns);
            }
        }

        private static UNoteSetting Load()
        {
            string value = EditorUserSettings.GetConfigValue(SettingKey);
            if (string.IsNullOrEmpty(value))
            {
                return new UNoteSetting();
            }

            return JsonUtility.FromJson<UNoteSetting>(value);
        }

        private static void Save(UNoteSetting uns)
        {
            string json = JsonUtility.ToJson(uns);
            EditorUserSettings.SetConfigValue(SettingKey, json);
        }
    }
}
