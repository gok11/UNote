using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public class UNoteSetting
    {

        #region Const
        
        private const string SettingKey = "UNote.UNoteSetting";
        
        #endregion // Const

        #region Field

        [SerializeField] private string m_userName;
        [SerializeField] private bool m_inspectorFoldoutOpened;

        #endregion // Field

        #region Constructor

        internal UNoteSetting()
        {
            m_userName = Environment.UserName;
        }

        #endregion

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
