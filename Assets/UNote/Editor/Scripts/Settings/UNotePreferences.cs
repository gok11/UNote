using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    [FilePath("UNotePreferences.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class UNotePreferences : ScriptableSingleton<UNotePreferences>
    {
        public string m_userName;
        public bool m_inspectorFoldoutOpened;
        public Vector2 m_attachmentImageMaxSize = new(InitialImageMaxSizeX, InitialImageMaxSizeY);

        private const float InitialImageMaxSizeX = 480;
        private const float InitialImageMaxSizeY = 270;
        
        private void OnEnable()
        {
            if (m_userName.IsNullOrWhiteSpace())
            {
                m_userName = Environment.UserName;
                Save();
            }

            if (m_attachmentImageMaxSize.x < InitialImageMaxSizeX ||
                m_attachmentImageMaxSize.y < InitialImageMaxSizeY)
            {
                m_attachmentImageMaxSize = new Vector2(InitialImageMaxSizeX, InitialImageMaxSizeY);
            }
        }

        public void Save()
        {
            Save(true);
        }
    }
}
