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

    public class UNoteEditorRightPane : UNoteEditorPaneBase
    {
        private NoteEditor m_noteEditor;

        private Label m_noteTitle;
        private ScrollView m_noteList;
        private TextField m_inputText;
        private ScrollView m_inputScroll;

        private Label m_authorLabel;
        private Button m_sendButton;

        private float m_lastScrollPosition;

        public UNoteEditorRightPane(NoteEditor noteEditor)
        {
            name = nameof(UNoteEditorRightPane);

            m_noteEditor = noteEditor;

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorRightPane
            );
            contentContainer.Add(tree.Instantiate());

            //
            m_noteTitle = contentContainer.Q<Label>("NoteTitle");
            m_noteList = contentContainer.Q<ScrollView>("NoteList");
            m_inputText = contentContainer.Q<TextField>("InputText");
            m_inputScroll = contentContainer.Q<ScrollView>("TextArea");

            m_inputText.BindProperty(m_noteEditor.Model.EditingText);

            m_authorLabel = contentContainer.Q<Label>("Author");
            m_sendButton = contentContainer.Q<Button>("SendButton");

            m_authorLabel.text = UserConfig.GetUNoteSetting().UserName;

            // CurrentNote と同じメモを時系列で並べる
            SetupNoteList();

            // ボタンを押したら新しいメモを作成
            m_sendButton.clicked += () =>
            {
                SendNote();
            };

            EditorApplication.update += () =>
            {
                float pos = m_inputText.cursorPosition.y;
                if (Mathf.Abs(pos - m_lastScrollPosition) > 0.1f)
                {
                    // Bound の更新を待つため1フレーム待つ
                    EditorApplication.delayCall += () =>
                    {
                        float min = 14.52f;
                        float max = m_inputText.worldBound.height - 4;

                        float rate = (pos - min) / (max - min);
                        float scrollMax = m_inputScroll.verticalScroller.highValue;
                        m_inputScroll.verticalScroller.value = rate * scrollMax;
                    };
                    m_lastScrollPosition = pos;
                }
            };

            contentContainer.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return && evt.shiftKey)
            {
                SendNote();
            }
        }

        private void SendNote()
        {
            if (string.IsNullOrWhiteSpace(m_inputText.value))
            {
                return;
            }

            // Undoに乗らないよう一時的にバインド解除
            m_inputText.Unbind();

            NoteBase note = EditorUNoteManager.CurrentRootNote;
            ProjectNote srcNote = note as ProjectNote;

            ProjectLeafNote projectLeafNote = EditorUNoteManager.AddNewLeafProjectNote(
                srcNote.NoteId,
                m_inputText.value
            );

            VisualElement newNoteElem = CreateNoteContentElement(projectLeafNote);
            m_noteList.Add(newNoteElem);
            EditorApplication.delayCall += () =>
            {
                m_noteList.ScrollTo(newNoteElem);
            };

            // バインドしなおす前に最新の状態にする
            m_noteEditor.Model.ModelObject.Update();
            m_noteEditor.Model.EditingText.stringValue = "";
            m_noteEditor.Model.ModelObject.ApplyModifiedProperties();

            m_inputText.BindProperty(m_noteEditor.Model.EditingText);

            // 中央ペインを更新
            m_noteEditor.CenterPane.SetupListItems(m_noteEditor);
        }

        private string GetNoteTitle(NoteBase note)
        {
            switch (note?.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    return ProjectNoteIDManager.ConvertGuid(projectNote.NoteId);
            }

            return string.Empty;
        }

        public void SetupNoteList()
        {
            // CurrentNote からタイトルを取得する
            NoteBase note = EditorUNoteManager.CurrentRootNote;
            m_noteTitle.text = GetNoteTitle(note);

            // 要素を追加し、その要素までスクロール
            VisualElement lastAddedElem = null;

            m_noteList.contentContainer.Clear();

            m_noteList.visible = false;

            switch (note?.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    string projectNoteId = projectNote.NoteId;
                    IEnumerable<ProjectLeafNote> leafNotes = EditorUNoteManager
                        .GetAllProjectLeafNotes()
                        .Where(t => t.NoteId == projectNoteId);

                    foreach (var leafNote in leafNotes)
                    {
                        lastAddedElem = CreateNoteContentElement(leafNote);
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

        public override void OnUndoRedo()
        {
            SetupNoteList();
        }
    }
}
