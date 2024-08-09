using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorContentElem : VisualElement
    {
        private NoteBase m_note;

        private Label m_contentText;
        private VisualElement m_editNoteElem;
        private Button m_contextButton;
        
        private TextField m_editField;
        private Button m_sendButton;

        public UNoteEditorContentElem(NoteBase note)
        {
            m_note = note;
            
            VisualTreeAsset noteContentTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteContent
            );

            VisualElement noteElement = noteContentTree.Instantiate();
            contentContainer.Add(noteElement);
            
            m_contentText = noteElement.Q<Label>("ContentText");
            m_editNoteElem = noteElement.Q<VisualElement>("EditNoteElem");
            m_contextButton = noteElement.Q<Button>("ContextButton");
            
            m_editField = noteElement.Q<TextField>("EditField");
            m_sendButton = noteElement.Q<Button>("SendButton");
            
            noteElement.Q<Label>("AuthorLabel").text = note.Author;
            noteElement.Q<Label>("CreatedDate").text = note.CreatedDate;

            bool isEdited = note.CreatedDate != note.UpdatedDate;
            m_contentText.text = note.NoteContent;

            if (isEdited)
            {
                m_contentText.text += CreateEditedText();
            }

            bool isOwnNote = note.Author == UNoteSetting.UserName;

            // register context button event
            m_contextButton.clicked += () =>
            {
                ShowContextMenu(note);
            };
            
            // edit event
            m_sendButton.clicked += FinishEditText;
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return && (evt.ctrlKey || evt.commandKey))
                {
                    FinishEditText();
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    QuitEditText();
                }
            }, TrickleDown.TrickleDown);

            // show menu if this is an own note
            if (isOwnNote)
            {
                noteElement.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    m_contextButton.visible = true;
                });

                noteElement.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    m_contextButton.visible = false;
                });

                noteElement.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button == 1)
                    {
                        ShowContextMenu(note);
                    }
                });
            }
        }
        
        private void ShowContextMenu(NoteBase note)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Edit"),
                false,
                EnableEditText);
            
            menu.AddItem(
                new GUIContent("Delete"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Do you want to delete this note?", "OK", "Cancel"))
                    {
                        EditorUNoteManager.DeleteNote(note);
                    }
                }
            );
            menu.ShowAsContext();
        }
        
        private void EnableEditText()
        {
            m_contentText.style.display = DisplayStyle.None;
            m_editNoteElem.style.display = DisplayStyle.Flex;
            m_contextButton.style.display = DisplayStyle.None;

            m_editField.value = m_note.NoteContent;
            m_editField.Focus();
        }

        private void QuitEditText()
        {
            m_contentText.style.display = DisplayStyle.Flex;
            m_editNoteElem.style.display = DisplayStyle.None;
            m_contextButton.style.display = DisplayStyle.Flex;
        }

        private void FinishEditText()
        {
            m_contentText.style.display = DisplayStyle.Flex;
            m_editNoteElem.style.display = DisplayStyle.None;
            m_contextButton.style.display = DisplayStyle.Flex;

            if (m_note.NoteContent != m_editField.value)
            {
                m_contentText.text = $"{m_editField.value} {CreateEditedText()}";

                m_note.NoteContent = m_editField.value;
                m_note.UpdatedDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                EditorUNoteManager.SaveAll();   
            }
        }

        private string CreateEditedText() => "<size=10><color=#999999> (edited)</color></size>";
    }
}
