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
        #region Field

        private UNoteEditor m_noteEditor;

        private Label m_noteTitle;
        private TextField m_titleField;

        private VisualElement m_subInfoArea;

        private ScrollView m_tabList;
        
        // Note list
        private ScrollView m_noteList;
        private VisualElement m_footerElem;
        
        // Note input field
        private NoteInputField m_noteInputField;

        private float m_lastScrollPosition;

        #endregion // Field

        #region Constructor
        internal UNoteEditorRightPane(UNoteEditor noteEditor)
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

            m_tabList = contentContainer.Q<ScrollView>("TabList");
            m_noteList = contentContainer.Q<ScrollView>("NoteList");

            NoteBase currentNote = EditorUNoteManager.CurrentNote;
            if (currentNote != null)
            {
                m_noteInputField = new NoteInputField(currentNote.NoteType, currentNote.NoteId);
                m_noteInputField.style.flexShrink = 0;
                contentContainer.Q<TemplateContainer>().Add(m_noteInputField);   
            }
            
            // Show note list by created date
            SetupNoteList();

            // Enter edit mode on click title label
            m_noteTitle.RegisterCallback<MouseDownEvent>(_ =>
            {
                EnableChangeTitleMode();
            });
            
            m_titleField.RegisterCallback<BlurEvent>(_ =>
            {
                SetTitleGUIEditMode(false);
            });

            // Register note event
            EditorUNoteManager.OnNoteAdded += _ => SetupNoteList();
            EditorUNoteManager.OnNoteDeleted += _ => SetupNoteList();
            
            EditorUNoteManager.OnNoteSelected += n =>
            {
                NoteBase note = n ?? EditorUNoteManager.CurrentNote;
                if (note != null)
                {
                    m_noteInputField?.SetNoteInfo(note.NoteType, note.NoteId);
                    SetupNoteList();   
                }
                else
                {
                    m_noteTitle.text = "";
                    m_noteList.contentContainer.Clear();
                }
            };
        }
        #endregion // Constructor

        #region Private Method

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
            
            bool isOwnNote = note.Author == UNoteSetting.UserName;

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
                    
                    // finish editing title
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

        private void SetupNoteList()
        {
            // disable to edit title
            SetTitleGUIEditMode(false);
            
            m_noteTitle.text = "";
            m_noteList.contentContainer.Clear();

            // set note info
            NoteBase note = EditorUNoteManager.CurrentNote;
            if (note == null)
            {
                return;
            }
            
            m_noteTitle.text = note.NoteName;
            
            // set sub info
            m_subInfoArea.Clear();
            SetSubInfoStyle(-1);
            
            switch (note.NoteType)
            {
                case NoteType.Project:
                    break;
                
                case NoteType.Asset:
                    AssetNote assetNote = note as AssetNote;
                    ObjectField assetField = new ObjectField("Reference Asset");
                    Object referenceAsset =
                        AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(assetNote.BindAssetId));
                    assetField.SetValueWithoutNotify(referenceAsset);
                    assetField.style.fontSize = 10;
                    assetField.SetEnabled(false);

                    m_subInfoArea.Add(assetField);
                    SetSubInfoStyle(1);
                    break;
            }

            // scroll to new elem
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

                    foreach (var leafNote in EditorUNoteManager.GetProjectLeafNoteListByNoteId(projectNoteId))
                    {
                        m_noteList.Insert(m_noteList.childCount - 1, new UNoteEditorContentElem(leafNote));
                    }
                    break;
                
                case NoteType.Asset:
                    AssetNote assetNote = note as AssetNote;
                    string assetNoteNoteId = assetNote?.NoteId;

                    foreach (var leafNote in EditorUNoteManager.GetAssetLeafNoteListByNoteId(assetNoteNoteId))
                    {
                        m_noteList.Insert(m_noteList.childCount - 1, new UNoteEditorContentElem(leafNote));
                    }
                    break;
                
                default:
                    throw new NotImplementedException();
            }
            
            // wait and scroll
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
                subInfoStyle.SetMargin(2,0,2,2);
                subInfoStyle.SetBorderWidth(1);
                subInfoStyle.height = 22 * lineCount;
            }
            else
            {
                subInfoStyle.SetMargin(0);
                subInfoStyle.SetBorderWidth(0);
                subInfoStyle.height = 0;
            }
        }

        #endregion // Private Method

        #region Undo

        internal override void OnUndoRedo(string undoName)
        {
            if (undoName.Contains("UNote Change Project Note Title"))
            {
                m_noteEditor.CenterPane.OnUndoRedo(undoName);
            }

            switch (EditorUNoteManager.CurrentNoteType)
            {
                case NoteType.Project:
                    EditorUNoteManager.ReloadProjectNotes();
                    break;
                
                case NoteType.Asset:
                    EditorUNoteManager.ReloadAssetNotes();
                    break;
            }
            
            SetupNoteList();
        }

        #endregion // Undo
        
    }
}
