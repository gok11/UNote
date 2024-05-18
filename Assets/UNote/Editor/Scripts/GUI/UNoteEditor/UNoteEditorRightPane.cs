using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Playables;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public partial class NoteEditorModel : ScriptableObject
    {
        public string editingText;

        public SerializedProperty EditingText => ModelObject.FindProperty(nameof(editingText));
    }

    public class UNoteEditorRightPane : VisualElement
    {
        private Label m_noteTitle;
        private ScrollView m_noteList;
        private TextField m_inputText;
        private Label m_authorLabel;
        private Button m_sendButton;

        public UNoteEditorRightPane(NoteEditor noteEditor)
        {
            name = nameof(UNoteEditorRightPane);

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorRightPane
            );
            contentContainer.Add(tree.Instantiate());

            //
            m_noteTitle = contentContainer.Q<Label>("NoteTitle");
            m_noteList = contentContainer.Q<ScrollView>("NoteList");
            m_inputText = contentContainer.Q<TextField>("InputText");

            m_inputText.BindProperty(noteEditor.Model.EditingText);

            m_authorLabel = contentContainer.Q<Label>("Author");
            m_sendButton = contentContainer.Q<Button>("SendButton");

            // CurrentNote からタイトルを取得する
            NoteBase note = EditorUNoteManager.CurrentNote;

            m_noteTitle.text = GetNoteTitle(note);
            m_authorLabel.text = UserConfig.GetUNoteSetting().UserName;

            // CurrentNote と同じメモを時系列で並べる
            SetupNoteList(note);

            // ボタンを押したら新しいメモを作成
            m_sendButton.clicked += () =>
            {
                ProjectNote srcNote = note as ProjectNote;
                ProjectNote projectNote = EditorUNoteManager.AddNewLeafProjectNote(
                    srcNote.ProjectNoteID,
                    m_inputText.value
                );

                VisualElement newNoteElem = CreateNoteContentElement(projectNote);
                m_noteList.Add(newNoteElem);
                EditorApplication.delayCall += () =>
                {
                    m_noteList.ScrollTo(newNoteElem);
                };

                m_inputText.value = "";
            };
        }

        private string GetNoteTitle(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    return ProjectNoteIDManager.ConvertGuid(projectNote.ProjectNoteID);
            }

            return string.Empty;
        }

        private void SetupNoteList(NoteBase note)
        {
            VisualElement lastAddedElem = null;

            m_noteList.contentContainer.Clear();

            m_noteList.visible = false;

            switch (note.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    string projectNoteId = projectNote.ProjectNoteID;
                    IEnumerable<ProjectNote> otherNotes = EditorUNoteManager
                        .GetAllProjectNotes()
                        .SelectMany(t => t)
                        .Where(t => !t.IsRootNote)
                        .Where(t => t.ProjectNoteID == projectNoteId);

                    foreach (var otherNote in otherNotes)
                    {
                        lastAddedElem = CreateNoteContentElement(otherNote);
                        m_noteList.Add(lastAddedElem);
                    }
                    break;
            }

            EditorApplication.delayCall += () =>
            {
                if (lastAddedElem != null)
                {
                    m_noteList.ScrollTo(lastAddedElem);
                }
                m_noteList.visible = true;
            };
        }

        private VisualElement CreateNoteContentElement(NoteBase note)
        {
            VisualTreeAsset noteContentTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteContent
            );

            VisualElement noteElement = noteContentTree.Instantiate();
            noteElement.Q<Label>("AuthorLabel").text = note.Author;
            noteElement.Q<Label>("UpdateDate").text = note.UpdatedDate;
            noteElement.Q<Label>("ContentText").text = note.NoteContent;
            return noteElement;
        }

        public void OnUndoRedo()
        {
            NoteBase note = EditorUNoteManager.CurrentNote;
            SetupNoteList(note);
        }
    }
}
