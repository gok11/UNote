using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public static class UNoteSetting
    {
        public static string UserName => UNotePreferences.instance.m_userName;

        public static bool InspectorFoldoutOpened
        {
            get => UNotePreferences.instance.m_inspectorFoldoutOpened;
            set
            {
                UNotePreferences.instance.m_inspectorFoldoutOpened = value;
                UNotePreferences.instance.Save();
            }
        }

        public static List<UNoteTagData> TagList => UNoteProjectSettings.instance.m_tagList;

        private const string LastSelectedQueryKey = "UNote.LastSelectedQuery";
        private static NoteQuery s_lastSelectedQuery;
        
        public static NoteQuery LastSelectedQuery
        {
            get
            {
                if (s_lastSelectedQuery != null)
                {
                    return s_lastSelectedQuery;
                }
                
                string queryId = EditorUserSettings.GetConfigValue(LastSelectedQueryKey);
                NoteQuery noteQuery = CustomQueryContainer.Get().NoteQueryList.Find(t => t.QueryID == queryId);
                return noteQuery;
            }
            set
            {
                s_lastSelectedQuery = value;
                string queryId = value != null ? value.QueryID : "";
                EditorUserSettings.SetConfigValue(LastSelectedQueryKey, queryId);
            }
        }
    }
}
