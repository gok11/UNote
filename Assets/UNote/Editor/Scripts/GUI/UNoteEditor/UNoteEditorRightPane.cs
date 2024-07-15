using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;

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

        private VisualElement m_subInfoArea;
        
        // Note list
        private ScrollView m_noteList;
        private VisualElement m_footerElem;
        
        // Note input field
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
                UxmlPath.NoteEditorRightPane
            );
            contentContainer.Add(tree.Instantiate());

            // 
            m_noteTitle = contentContainer.Q<Label>("NoteTitle");
            m_titleField = contentContainer.Q<TextField>("TitleField");

            m_subInfoArea = contentContainer.Q<VisualElement>("NoteSubInfoArea");
            
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
            
            m_titleField.RegisterCallback<BlurEvent>(_ =>
            {
                SetTitleGUIEditMode(false);
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
            
            // Register note event
            EditorUNoteManager.OnNoteAdded += note => SetupNoteList();
            EditorUNoteManager.OnNoteSelected += note => SetupNoteList();
            EditorUNoteManager.OnNoteDeleted += note => SetupNoteList();
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

            NoteBase note = EditorUNoteManager.CurrentNote;
            NoteBase newLeafNote = null;

            switch (note.NoteType)
            {
                case NoteType.Project:
                    if (note is ProjectNote projectNote)
                    {
                        newLeafNote = EditorUNoteManager.AddNewLeafProjectNote(
                            projectNote.NoteId,
                            m_inputText.value
                        );   
                    }
                    break;
                
                case NoteType.Asset:
                    if (note is AssetLeafNote assetNote)
                    {
                        newLeafNote = EditorUNoteManager.AddNewLeafAssetNote(
                            assetNote.NoteId,
                            m_inputText.value
                        );
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (newLeafNote == null)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            EditorApplication.delayCall += () =>
            {
                m_noteList.ScrollTo(m_footerElem);
            };

            // バインドしなおす前に最新の状態にする
            m_noteEditor.Model.ModelObject.Update();
            m_noteEditor.Model.EditingText.stringValue = "";
            m_noteEditor.Model.ModelObject.ApplyModifiedProperties();

            m_inputText.BindProperty(m_noteEditor.Model.EditingText);
        }

        private void EnableChangeTitleMode()
        {
            NoteBase note = EditorUNoteManager.CurrentNote;
            if (note == null)
            {
                return;
            }
            
            if (note.NoteType != NoteType.Project)
            {
                return;
            }
            
            bool isOwnNote = note.Author == UserConfig.GetUNoteSetting().UserName;

            if (!isOwnNote)
            {
                return;
            }
            
            SetTitleGUIEditMode(true);

            m_titleField.value = note.NoteName;
            m_titleField.Focus();
            
            m_titleField.UnregisterCallback<KeyDownEvent>(TryChangeTitle);
            m_titleField.RegisterCallback<KeyDownEvent>(TryChangeTitle);

            void TryChangeTitle(KeyDownEvent evt)
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    EditorUNoteManager.ChangeNoteName(note, m_titleField.value);
                    
                    // 編集を終え、テキスト更新
                    SetTitleGUIEditMode(false);

                    m_noteTitle.text = EditorUNoteManager.CurrentNote.NoteName;
                    
                    m_noteEditor.CenterPane.SetupListItems();
                }

                
                if (evt.keyCode == KeyCode.Escape)
                {
                    m_titleField.UnregisterCallback<KeyDownEvent>(TryChangeTitle);

                    SetTitleGUIEditMode(false);
                }
            }
        }
        
        public void SetupNoteList()
        {
            // メモの表示状態をリセットする
            SetTitleGUIEditMode(false);
            
            m_noteTitle.text = "";
            m_noteList.contentContainer.Clear();

            // メモの情報を設定
            NoteBase note = EditorUNoteManager.CurrentNote;
            if (note == null)
            {
                return;
            }
            
            m_noteTitle.text = note.NoteName;
            
            // サブ情報の設定
            m_subInfoArea.Clear();
            SetSubInfoStyle(-1);
            
            switch (note.NoteType)
            {
                case NoteType.Project:
                    break;
                
                case NoteType.Asset:
                    ObjectField assetField = new ObjectField("Reference Asset");
                    Object referenceAsset =
                        AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(note.NoteId));
                    assetField.SetValueWithoutNotify(referenceAsset);
                    assetField.style.fontSize = 10;
                    assetField.SetEnabled(false);

                    m_subInfoArea.Add(assetField);
                    SetSubInfoStyle(1);
                    break;
            }

            // 要素を追加し、その要素までスクロール
            m_noteList.visible = false;

            m_footerElem = new VisualElement();
            m_footerElem.name = "footer_elem";
            m_footerElem.style.height = 0;
            m_noteList.Add(m_footerElem);

            switch (note.NoteType)
            {
                case NoteType.Project:
                    ProjectNote projectNote = note as ProjectNote;
                    string projectNoteId = projectNote?.NoteId;

                    foreach (var leafNote in EditorUNoteManager.GetProjectLeafNoteListByProjectNoteId(projectNoteId))
                    {
                        m_noteList.Insert(m_noteList.childCount - 1, new UNoteEditorContentElem(leafNote));
                    }
                    break;
                
                case NoteType.Asset:
                    AssetLeafNote assetNote = note as AssetLeafNote;
                    string assetNoteNoteId = assetNote?.NoteId;

                    foreach (var leafNote in EditorUNoteManager.GetAssetLeafNoteListByNoteId(assetNoteNoteId))
                    {
                        m_noteList.Insert(m_noteList.childCount - 1, new UNoteEditorContentElem(leafNote));
                    }
                    break;
                
                default:
                    throw new NotImplementedException();
            }
            
            // スクロールが正しくできるよう待つ
            EditorApplication.delayCall += () =>
            EditorApplication.delayCall += () =>
            {
                if (m_noteList != null)
                {
                    if (m_noteList.Contains(m_footerElem))
                    {
                        m_noteList.ScrollTo(m_footerElem);   
                    }
                    m_noteList.visible = true;   
                }
            };
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

        private void SetSubInfoStyle(int lineCount)
        {
            IStyle subInfoStyle = m_subInfoArea.style;
            
            if (lineCount > 0)
            {
                subInfoStyle.marginTop = subInfoStyle.marginLeft = subInfoStyle.marginRight= 2;
                subInfoStyle.borderTopWidth = subInfoStyle.borderBottomWidth =
                    subInfoStyle.borderLeftWidth = subInfoStyle.borderRightWidth = 1;
                subInfoStyle.height = 22 * lineCount;
            }
            else
            {
                subInfoStyle.marginTop = subInfoStyle.marginLeft = subInfoStyle.marginRight= 0;
                subInfoStyle.borderTopWidth = subInfoStyle.borderBottomWidth =
                    subInfoStyle.borderLeftWidth = subInfoStyle.borderRightWidth = 0;
                subInfoStyle.height = 0;
            }
        }
        
        public override void OnUndoRedo(string undoName)
        {
            if (undoName.Contains("UNote Change Project Note Title"))
            {
                m_noteEditor.CenterPane.OnUndoRedo(undoName);
            }
            SetupNoteList();
        }
    }
}
