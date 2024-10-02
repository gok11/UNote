using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public class UNoteTagData
    {
        [SerializeField] private string m_tagId;
        [SerializeField] private string m_tagName;
        [SerializeField] private Color m_color;

        public string TagId
        {
            get => m_tagId;
            set => m_tagId = value;
        }

        public string TagName
        {
            get => m_tagName;
            set => m_tagName = value;
        }

        public Color Color
        {
            get => m_color;
            set => m_color = value;
        }

        public UNoteTagData(string tagName, Color color)
        {
            TagId = Guid.NewGuid().ToString();
            TagName = tagName;
            Color = color;
        }

        public void Initialize()
        {
            m_tagId = Guid.NewGuid().ToString();
            m_tagName = "NewTag";
            Color = Color.white;
        }
    }
}
