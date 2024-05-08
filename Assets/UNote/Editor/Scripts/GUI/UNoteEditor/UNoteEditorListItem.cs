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
        private VisualElement m_parentContainer;

        private NoteBase m_note;

        private Label m_nameLabel;
        private Label m_updateDateLabel;
        private Label m_noteContentLabel;

        public UNoteEditorListItem()
        {
            VisualTreeAsset listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteListItem
            );
            TemplateContainer template = listItem.CloneTree();

            m_nameLabel = template.Q<Label>("Name");
            m_updateDateLabel = template.Q<Label>("UpdateDate");
            m_noteContentLabel = template.Q<Label>("ContentLine");

            contentContainer.Add(template);

            // マウスイベント
            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(
                        new GUIContent("Delete Note"),
                        false,
                        () =>
                        {
                            EditorUNoteManager.DeleteNote(m_note);
                            parent.Remove(this);
                        }
                    );
                    menu.ShowAsContext();
                }

                evt.StopPropagation();
            });
        }

        public void Setup(NoteBase note)
        {
            m_note = note;

            if (note is ProjectNote projectNote)
            {
                m_nameLabel.text = projectNote.Title;
            }

            m_updateDateLabel.text = DateTime.Parse(note.UpdatedDate).ToString("yyyy-MM-dd");
            m_noteContentLabel.text = note.NoteContent;
        }
    }
}
