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
        private Button m_contextButton;
        private VisualElement m_noteListItem;

        public NoteBase BindNote => m_note;

        public UNoteEditorListItem(UNoteEditor noteEditor)
        {
            VisualTreeAsset listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteListItem
            );
            TemplateContainer template = listItem.CloneTree();

            m_nameLabel = template.Q<Label>("Name");
            m_noteContentLabel = template.Q<Label>("ContentLine");
            m_contextButton = template.Q<Button>("ContextButton");

            contentContainer.Add(template);

            m_noteListItem = contentContainer.Q("NoteListItem");

            m_contextButton.visible = false;

            // Handle mouse event
            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (EditorUNoteManager.CurrentRootNote != m_note)
                {
                    EditorUNoteManager.SelectRoot(m_note);
                    noteEditor.RightPane.SetupNoteList();
                    noteEditor.CenterPane.UpdateNoteList();
                }

                bool isOwnNote = m_note.Author == UserConfig.GetUNoteSetting().UserName;
                if (isOwnNote)
                {
                    switch (evt.button)
                    {
                        case 1:
                            ShowContextMenu();
                            break;
                    }
                }

                evt.StopPropagation();
            });

            contentContainer.RegisterCallback<MouseEnterEvent>(_ =>
            {
                bool isOwnNote = m_note.Author == UserConfig.GetUNoteSetting().UserName;
                if (isOwnNote)
                {
                    m_contextButton.visible = true;
                }
            });

            contentContainer.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                bool isOwnNote = m_note.Author == UserConfig.GetUNoteSetting().UserName;
                if (isOwnNote)
                {
                    m_contextButton.visible = false;
                }
            });

            // Handle button event
            m_contextButton.clicked += () =>
            {
                ShowContextMenu();
            };
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

        private void ShowContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Delete Note"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Do you want to delete this note?", "OK", "Cancel"))
                    {
                        EditorUNoteManager.DeleteNote(m_note);
                        parent.Remove(this);   
                    }
                }
            );

            string archiveLabel = m_note.Archived ? "Unarchived" : "Archive";
            menu.AddItem(
                new GUIContent(archiveLabel),
                false,
                () =>
                {
                    EditorUNoteManager.ToggleArchived(m_note);
                }
            );
            menu.ShowAsContext();
        }
    }
}
