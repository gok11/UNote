using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public class UNoteSetting
    {
        private const string SettingKey = "UNote.UNoteSetting";
        
        [SerializeField] private string m_userName;
        [SerializeField] private bool m_inspectorFoldoutOpened;
        [SerializeField] private List<NoteQuery> m_noteQueryList;

        #region Property

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

        public static List<NoteQuery> NoteQueryList => Load().m_noteQueryList;

        #endregion // Property

        #region Private Static Methods

        private static UNoteSetting Load()
        {
            string value = EditorUserSettings.GetConfigValue(SettingKey);
            if (string.IsNullOrEmpty(value))
            {
                return new UNoteSetting();
            }

            return JsonUtility.FromJson<UNoteSetting>(value);
        }

        internal static void Save(UNoteSetting uns)
        {
            string json = JsonUtility.ToJson(uns);
            EditorUserSettings.SetConfigValue(SettingKey, json);
        }

        #endregion // Private Static Methods
    }
}
