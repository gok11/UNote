using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorContentElem : VisualElement
    {
        private UNoteEditor m_noteEditor;
        private NoteBase m_note;

        private Label m_contentText;
        private VisualElement m_editNoteElem;
        private Button m_contextButton;
        
        private TextField m_editField;
        private Button m_sendButton;

        public UNoteEditorContentElem(UNoteEditor noteEditor, NoteBase note)
        {
            m_noteEditor = noteEditor;
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
            noteElement.Q<Label>("UpdateDate").text = note.UpdatedDate;
            m_contentText.text = note.NoteContent;

            bool isOwnNote = note.Author == UserConfig.GetUNoteSetting().UserName;

            // コンテキストボタンイベント
            m_contextButton.clicked += () =>
            {
                ShowContextMenu(note);
            };
            
            // 編集イベント
            m_sendButton.clicked += FinishEditText;
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return && evt.shiftKey)
                {
                    FinishEditText();
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    QuitEditText();
                }
            }, TrickleDown.TrickleDown);

            // 自分のメモなら編集等のメニューを出す
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
                () =>
                {
                    EnableEditText();
                });
            
            menu.AddItem(
                new GUIContent("Delete"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Do you want to delete this note?", "OK", "Cancel"))
                    {
                        EditorUNoteManager.DeleteNote(note);
                        m_noteEditor.RightPane.SetupNoteList();   
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

            m_editField.value = m_contentText.text;
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

            m_contentText.text = m_editField.value;

            m_note.NoteContent = m_editField.value;
            EditorUNoteManager.SaveAll();
        }
    }
}
