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
        public List<UNoteTagData> m_tagList;
        private SerializedObject m_serializedObject;

        internal SerializedObject SerializedObject => m_serializedObject ?? new SerializedObject(this);

        private void OnEnable()
        {
            if (m_tagList == null || m_tagList.Count == 0)
            {
                m_tagList = new List<UNoteTagData>
                {
                    new ("Todo", StyleUtil.TodoInitColor),
                    new ("Bug", StyleUtil.BugInitColor)
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
