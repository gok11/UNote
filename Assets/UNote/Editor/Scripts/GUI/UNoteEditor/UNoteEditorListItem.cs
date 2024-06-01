using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

            m_nameLabel.text = note.NoteName;

            switch (note.NoteType)
            {
                case NoteType.Project:
                    ProjectLeafNote leafNote = EditorUNoteManager
                        .GetAllProjectLeafNotes()
                        .Where(t => t.NoteId == note.NoteId)
                        .OrderByDescending(t => t.UpdatedDate)
                        .FirstOrDefault();

                    m_noteContentLabel.text = leafNote
                        ?.NoteContent.Replace("\r", " ")
                        .Replace("\n", " ");
                    break;
            }
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
