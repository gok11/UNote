using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorRightPane : VisualElement
    {
        private Label m_noteTitle;
        private TextField m_inputText;
        private Button m_sendButton;

        public UNoteEditorRightPane()
        {
            name = nameof(UNoteEditorRightPane);

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorRightPane
            );
            contentContainer.Add(tree.Instantiate());

            //
            m_noteTitle = contentContainer.Q<Label>("NoteTitle");
            m_inputText = contentContainer.Q<TextField>("InputText");
            m_sendButton = contentContainer.Q<Button>("SendButton");

            // CurrentNote からタイトルを取得する
            NoteBase note = EditorUNoteManager.CurrentNote;

            switch (note.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    string noteName = ProjectNoteIDManager.ConvertGuid(projectNote.ProjectNoteID);
                    m_noteTitle.text = noteName;
                    break;
            }

            m_sendButton.clicked += () => {
                // TODO メモリストに新しい IsRoot false のメモを追加
            };
        }
    }
}
