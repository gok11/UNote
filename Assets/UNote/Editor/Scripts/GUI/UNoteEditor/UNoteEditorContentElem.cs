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

        private Label m_contentText;
        private VisualElement m_editNoteElem;
        private TextField m_editField;

        public UNoteEditorContentElem(UNoteEditor noteEditor, NoteBase note)
        {
            m_noteEditor = noteEditor;
            
            VisualTreeAsset noteContentTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteContent
            );

            VisualElement noteElement = noteContentTree.Instantiate();
            contentContainer.Add(noteElement);
            
            m_contentText = noteElement.Q<Label>("ContentText");
            m_editNoteElem = noteElement.Q<VisualElement>("EditNoteElem");
            m_editField = noteElement.Q<TextField>("EditField");
            
            noteElement.Q<Label>("AuthorLabel").text = note.Author;
            noteElement.Q<Label>("UpdateDate").text = note.UpdatedDate;
            m_contentText.text = note.NoteContent;

            bool isOwnNote = note.Author == UserConfig.GetUNoteSetting().UserName;

            Button contextButton = noteElement.Q<Button>("ContextButton");
            contextButton.clicked += () =>
            {
                ShowContextMenu(note);
            };

            // 自分のメモなら編集等のメニューを出す
            if (isOwnNote)
            {
                noteElement.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    contextButton.visible = true;
                });

                noteElement.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    contextButton.visible = false;
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

            m_editField.value = m_contentText.text;
        }
    }
}
