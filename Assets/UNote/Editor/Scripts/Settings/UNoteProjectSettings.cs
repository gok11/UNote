using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    [FilePath("ProjectSettings/UNoteSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class UNoteProjectSettings : ScriptableSingleton<UNoteProjectSettings>
    {
        public List<UNoteTag> m_tagList;

        private void OnEnable()
        {
            if (m_tagList == null || m_tagList.Count == 0)
            {
                m_tagList = new List<UNoteTag>
                {
                    new ("Todo", Color.blue),
                    new ("Bug", Color.red)
                };
                Save();
            }
        }

        public void Save()
        {
            Save(true);
        }
    }
}
