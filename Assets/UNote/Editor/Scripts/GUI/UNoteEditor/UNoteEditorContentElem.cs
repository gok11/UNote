using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    /// <summary>
    /// NoteEditor part VisualElement. Message display.
    /// </summary>
    public class UNoteEditorContentElem : VisualElement
    {
        private NoteMessageBase m_message;

        private VisualElement m_noteTag;
        private Button m_addTagButton;
        private VisualElement m_contents;
        private VisualElement m_editNoteElem;
        private Button m_contextButton;
        
        private TextField m_editField;
        private Button m_addButton;
        private Button m_sendButton;

        internal UNoteEditorContentElem(NoteMessageBase message)
        {
            m_message = message;
            
            // Get VisualElements
            VisualTreeAsset noteContentTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteContent
            );

            VisualElement noteElement = noteContentTree.Instantiate();
            contentContainer.Add(noteElement);

            m_noteTag = noteElement.Q("NoteTag");
            m_addTagButton = noteElement.Q<Button>("AddTagButton");
            m_contents = noteElement.Q("Contents");
            m_editNoteElem = noteElement.Q<VisualElement>("EditNoteElem");
            m_contextButton = noteElement.Q<Button>("ContextButton");
            
            m_editField = noteElement.Q<TextField>("EditField");
            m_addButton = noteElement.Q<Button>("AddButton");
            m_sendButton = noteElement.Q<Button>("SendButton");
            
            noteElement.Q<Label>("AuthorLabel").text = message.Author;
            noteElement.Q<Label>("CreatedDate").text = message.CreatedDate;
            
            // Set style
            m_contents.style.SetPadding(3, 4, 3, 8);
            m_contents.style.whiteSpace = WhiteSpace.Normal;
            
            m_addTagButton.Q("Icon").style.backgroundImage 
                = AssetDatabase.LoadAssetAtPath<Texture2D>(PathUtil.GetTexturePath("tag.png"));
            
            // Load tags
            LoadTags(m_noteTag);
            
            // Parse text and insert elem
            NoteTextParser.ParseToElements(m_message, m_contents);

            // register button event
            m_addTagButton.clicked += () =>
            {
                VisualElementUtil.ShowAddTagMenu(m_editNoteElem);
            };
            
            m_contextButton.clicked += () =>
            {
                ShowContextMenu(message);
            };
            
            // edit event
            m_addButton.clicked += ShowAddMenu;
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
            
            // Prepare drop area
            VisualElementUtil.CreateDropAreaElem(m_editField);

            // show menu if this is an own note
            bool isOwnNote = message.Author == UNoteSetting.UserName;
            
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
                        ShowContextMenu(message);
                    }
                });
            }
        }

        /// <summary>
        /// Message edit menu
        /// </summary>
        /// <param name="note"></param>
        private void ShowContextMenu(NoteBase note)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Edit"),
                false,
                EnableEditText
            );
            
            menu.AddSeparator("");
            
            menu.AddItem(
                new GUIContent("Copy ID"),
                false,
                () =>
                {
                    EditorGUIUtility.systemCopyBuffer = $"nid:{m_message.ReferenceNoteId}";
                }
            );
            
            menu.AddItem(
                new GUIContent("Copy Text"),
                false,
                () =>
                {
                    EditorGUIUtility.systemCopyBuffer = m_message.NoteContent;
                }
            );
            
            menu.AddSeparator("");
            
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

        /// <summary>
        /// Load tags for current message
        /// </summary>
        /// <param name="container"></param>
        /// <param name="allowTagRemove"></param>
        private void LoadTags(VisualElement container, bool allowTagRemove = false)
        {
            VisualElement tags = container.Q("Tags");
            tags.Clear();

            foreach (var id in m_message.NoteTagDataIdList)
            {
                tags.Add(new UNoteTag(id, allowTagRemove));
            }
        }
        
        /// <summary>
        /// Enter edit mode
        /// </summary>
        private void EnableEditText()
        {
            m_noteTag.style.display = DisplayStyle.None;
            m_contents.style.display = DisplayStyle.None;
            m_editNoteElem.style.display = DisplayStyle.Flex;
            m_contextButton.style.display = DisplayStyle.None;

            LoadTags(m_editNoteElem, true);
            
            m_editField.value = m_message.NoteContent;
            m_editField.Focus();
        }

        /// <summary>
        /// Cancel edit mode
        /// </summary>
        private void QuitEditText()
        {
            m_noteTag.style.display = DisplayStyle.Flex;
            m_contents.style.display = DisplayStyle.Flex;
            m_editNoteElem.style.display = DisplayStyle.None;
            m_contextButton.style.display = DisplayStyle.Flex;
            
            LoadTags(m_noteTag);
        }

        /// <summary>
        /// Finish edit mode
        /// </summary>
        private void FinishEditText()
        {
            m_noteTag.style.display = DisplayStyle.Flex;
            m_contents.style.display = DisplayStyle.Flex;
            m_editNoteElem.style.display = DisplayStyle.None;
            m_contextButton.style.display = DisplayStyle.Flex;
            
            // Update tag
            List<string> tagIdList = new List<string>();
            foreach (var noteTag in m_editNoteElem.Q("Tags").Query<UNoteTag>().Build())
            {
                tagIdList.Add(noteTag.TagId);
            }
            m_message.NoteTagDataIdList = tagIdList;
            
            LoadTags(m_noteTag);
            UNoteEditor.CenterPane.SetupListItems();

            // Update text
            if (m_message.NoteContent != m_editField.value)
            {
                m_message.NoteContent = m_editField.value;

                // Reset field
                m_contents.Clear();
                NoteTextParser.ParseToElements(m_message, m_contents);
                
                // Update note last updated date
                m_message.UpdatedDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                EditorUNoteManager.SaveAll();
                    
                UNoteEditor.CenterPane?.SetupListItems();
            }
        }
        
        /// <summary>
        /// Add content text menu
        /// </summary>
        internal void ShowAddMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Reference/Asset"), false, () =>
            {
                string filePath = EditorUtility.OpenFilePanel("Select Asset", Application.dataPath, "*");
                ParsePathToGuidAndInput(filePath);
            });
            
            menu.AddItem(new GUIContent("Reference/Folder"), false, () =>
            {
                string folderPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
                ParsePathToGuidAndInput(folderPath);
            });
            
            menu.AddItem(new GUIContent("Screenshot/GameView"), false, () =>
            {
                string ssSavePath = PathUtil.GetScreenshotSavePath();
                Utility.TakeScreenShot(ScreenshotTarget.GameView, ssSavePath);
                ParsePathToGuidAndInput(ssSavePath);
            });
            
            menu.AddItem(new GUIContent("Screenshot/SceneView"), false, () =>
            {
                string ssSavePath = PathUtil.GetScreenshotSavePath();
                Utility.TakeScreenShot(ScreenshotTarget.SceneView, ssSavePath);
                ParsePathToGuidAndInput(ssSavePath);
            });
            
            menu.ShowAsContext();
        }
        
        /// <summary>
        /// Parse file or folder path to guid and insert it to InputText
        /// </summary>
        /// <param name="path"></param>
        void ParsePathToGuidAndInput(string path)
        {
            string guid = AssetDatabase.AssetPathToGUID(path.FullPathToAssetPath());
            if (!string.IsNullOrEmpty(guid))
            {
                if (!m_editField.value.IsNullOrEmpty())
                {
                    m_editField.value += "\n";
                }
                m_editField.value += $"[unatt-guid:{guid}]";
            }
        }

        private string CreateEditedText() => "<size=10><color=#999999> (edited)</color></size>";

        private Label CreateEditedTextElem() => new(CreateEditedText());
    }
}
