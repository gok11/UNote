using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorListItem : VisualElement
    {
        private TemplateContainer m_templateContainer;

        private Label m_nameLabel;
        private Label m_updateDateLabel;
        private Label m_noteContentLabel;

        public UNoteEditorListItem()
        {
            VisualTreeAsset listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteListItem
            );
            m_templateContainer = listItem.CloneTree();

            m_nameLabel = m_templateContainer.Q<Label>("Name");
            m_updateDateLabel = m_templateContainer.Q<Label>("UpdateDate");
            m_noteContentLabel = m_templateContainer.Q<Label>("ContentLine");

            contentContainer.Add(m_templateContainer);
        }

        public void Setup(NoteBase note, VisualElement parentContainer)
        {
            ProjectNote pNote = note as ProjectNote;

            m_nameLabel.text = pNote.Title;
            m_updateDateLabel.text = DateTime.Parse(note.UpdatedDate).ToString("yyyy-MM-dd");
            m_noteContentLabel.text = note.NoteContent;

            parentContainer.Add(this);
        }
    }
}
