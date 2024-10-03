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

        public static Vector2 AttachmentImageMaxSize =>
            UNotePreferences.instance.m_attachmentImageMaxSize;

        public static List<UNoteTagData> TagList => UNoteProjectSettings.instance.m_tagList;
    }
}
