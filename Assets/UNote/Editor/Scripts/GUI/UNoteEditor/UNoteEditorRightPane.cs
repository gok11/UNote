using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Button = UnityEngine.UIElements.Button;

namespace UNote.Editor
{
    public partial class NoteEditorModel : ScriptableObject
    {
        public string editingText;

        public SerializedProperty EditingText => ModelObject.FindProperty(nameof(editingText));
    }

    public class UNoteEditorRightPane : UNoteEditorPaneBase
    {
        private UNoteEditor m_noteEditor;

        private Label m_noteTitle;
        private TextField m_titleField;
        private Button m_openButton;
        
        private ScrollView m_noteList;
        private TextField m_inputText;
        private ScrollView m_inputScroll;

        private Label m_authorLabel;
        private Button m_sendButton;

        private float m_lastScrollPosition;

        public UNoteEditorRightPane(UNoteEditor noteEditor)
        {
            name = nameof(UNoteEditorRightPane);

            m_noteEditor = noteEditor;

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorRightPane
            );
            contentContainer.Add(tree.Instantiate());

            //
            m_noteTitle = contentContainer.Q<Label>("NoteTitle");
            m_titleField = contentContainer.Q<TextField>("TitleField");
            m_openButton = contentContainer.Q<Button>("OpenButton");
            
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
            m_sendButton.clicked += SendNote;
            
            // 自分が所有するメモならタイトルクリックで編集モードにする
            m_noteTitle.RegisterCallback<MouseDownEvent>(_ =>
            {
                EnableChangeTitleMode();
            });
            
            // テキストフィールド内のカーソル移動に合わせてスクロールする
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
                evt.StopPropagation();
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

            if (srcNote == null)
            {
                return;
            }

            ProjectLeafNote projectLeafNote = EditorUNoteManager.AddNewLeafProjectNote(
                srcNote.NoteId,
                m_inputText.value
            );

            VisualElement newNoteElem = CreateNoteContentElement(projectLeafNote);
            m_noteList.Add(newNoteElem);
            EditorApplication.delayCall += () =>
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
            m_noteEditor.CenterPane.SetupListItems();
        }

        private void EnableChangeTitleMode()
        {
            NoteBase note = EditorUNoteManager.CurrentRootNote;
            bool isOwnNote = note.Author == UserConfig.GetUNoteSetting().UserName;

            if (!isOwnNote)
            {
                return;
            }
            
            SetTitleGUIEditMode(true);

            m_titleField.value = GetNoteTitle(note);
            m_titleField.Focus();
            
            m_titleField.UnregisterCallback<KeyDownEvent>(TryChangeTitle);
            m_titleField.RegisterCallback<KeyDownEvent>(TryChangeTitle);

            void TryChangeTitle(KeyDownEvent evt)
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    switch (note?.NoteType)
                    {
                        case NoteType.Project:
                            ProjectNote projectNote = note as ProjectNote;
                            string projectNoteId = projectNote?.NoteId;
                            EditorUNoteManager.ChangeProjectNoteTitle(projectNoteId, m_titleField.value);
                            break;
                    }
                    
                    // 編集を終え、テキスト更新
                    SetTitleGUIEditMode(false);

                    m_noteTitle.text = GetNoteTitle(EditorUNoteManager.CurrentRootNote);
                    
                    m_noteEditor.CenterPane.SetupListItems();
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    m_titleField.UnregisterCallback<KeyDownEvent>(TryChangeTitle);

                    SetTitleGUIEditMode(false);
                }
            }
        }

        private string GetNoteTitle(NoteBase note)
        {
            switch (note?.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    return ProjectNoteIDManager.ConvertGuid(projectNote?.NoteId);
            }

            return string.Empty;
        }

        private void SetTitleGUIEditMode(bool enableEdit)
        {
            if (enableEdit)
            {
                m_noteTitle.style.display = DisplayStyle.None;
                m_titleField.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_noteTitle.style.display = DisplayStyle.Flex;
                m_titleField.style.display = DisplayStyle.None;
            }
        }
        
        public void SetupNoteList()
        {
            // メモの表示状態をリセットする
            SetTitleGUIEditMode(false);
            
            // TODO 外部アセット等を参照するメモならそれを開くボタンを表示
            
            // CurrentNote からタイトルを取得する
            NoteBase note = EditorUNoteManager.CurrentRootNote;
            m_noteTitle.text = GetNoteTitle(note);

            // 要素を追加し、その要素までスクロール
            m_noteList.contentContainer.Clear();

            m_noteList.visible = false;

            VisualElement footerElem = new VisualElement();
            footerElem.name = "footer_elem";
            footerElem.style.height = 0;
            m_noteList.Add(footerElem);

            switch (note?.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    string projectNoteId = projectNote?.NoteId;
                    IEnumerable<ProjectLeafNote> leafNotes = EditorUNoteManager
                        .GetAllProjectLeafNotes()
                        .Where(t => t.NoteId == projectNoteId);

                    foreach (var leafNote in leafNotes)
                    {
                        m_noteList.Insert(m_noteList.childCount - 1, CreateNoteContentElement(leafNote));
                    }
                    break;
            }
            
            // スクロールが正しくできるよう待つ
            EditorApplication.delayCall += () =>
            EditorApplication.delayCall += () =>
            {
                if (m_noteList != null)
                {
                    m_noteList.ScrollTo(footerElem);
                    m_noteList.visible = true;   
                }
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

            return noteElement;
        }

        private void ShowContextMenu(NoteBase note)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Delete Note"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Do you want to delete this note?", "OK", "Cancel"))
                    {
                        EditorUNoteManager.DeleteNote(note);
                        SetupNoteList();   
                    }
                }
            );
            menu.ShowAsContext();
        }

        public override void OnUndoRedo()
        {
            SetupNoteList();
        }
    }
}
