using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public class UNoteTag
    {
        public string m_tagName;
        public Color m_color;

        public UNoteTag(string tagName, Color color)
        {
            m_tagName = tagName;
            m_color = color;
        }
    }
}
