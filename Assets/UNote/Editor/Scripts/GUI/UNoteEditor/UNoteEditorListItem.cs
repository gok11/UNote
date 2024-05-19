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
        private VisualElement m_noteListItem;

        public NoteBase BindNote => m_note;

        public UNoteEditorListItem(NoteEditor noteEditor)
        {
            VisualTreeAsset listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteListItem
            );
            TemplateContainer template = listItem.CloneTree();

            m_nameLabel = template.Q<Label>("Name");
            m_updateDateLabel = template.Q<Label>("UpdateDate");
            m_noteContentLabel = template.Q<Label>("ContentLine");

            contentContainer.Add(template);

            m_noteListItem = contentContainer.Q("NoteListItem");

            // マウスイベント
            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                switch (evt.button)
                {
                    case 0:
                        if (EditorUNoteManager.CurrentRootNote != m_note)
                        {
                            EditorUNoteManager.SelectRoot(m_note);
                            noteEditor.RightPane.SetupNoteList();
                            noteEditor.CenterPane.UpdateNoteList();
                        }
                        break;

                    case 1:
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
                        break;
                }

                evt.StopPropagation();
            });
        }

        public void Setup(NoteBase note)
        {
            m_note = note;

            if (note is ProjectNote projectNote)
            {
                m_nameLabel.text = ProjectNoteIDManager.ConvertGuid(projectNote.NoteId);
            }

            m_updateDateLabel.text = DateTime.Parse(note.UpdatedDate).ToString("yyyy-MM-dd");
            m_noteContentLabel.text = note.NoteContent;
            Focus();
        }

        public void SelectItem(bool select)
        {
            if (select)
            {
                m_noteListItem.style.backgroundColor = StyleUtil.SelectColor;
            }
            else
            {
                m_noteListItem.style.backgroundColor = StyleUtil.UnselectColor;
            }
        }
    }
}
