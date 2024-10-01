using System;
using System.Globalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    public class UNoteEditorContentElem : VisualElement
    {
        #region Field

        private NoteBase m_note;

        private VisualElement m_noteTag;
        private VisualElement m_contents;
        private VisualElement m_editNoteElem;
        private Button m_contextButton;
        
        private TextField m_editField;
        private Button m_addButton;
        private Button m_sendButton;

        #endregion // Field

        #region Constructor

        internal UNoteEditorContentElem(NoteBase note)
        {
            m_note = note;
            
            VisualTreeAsset noteContentTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteContent
            );

            VisualElement noteElement = noteContentTree.Instantiate();
            contentContainer.Add(noteElement);

            m_noteTag = noteElement.Q("NoteTag");
            m_contents = noteElement.Q("Contents");
            m_editNoteElem = noteElement.Q<VisualElement>("EditNoteElem");
            m_contextButton = noteElement.Q<Button>("ContextButton");
            
            m_editField = noteElement.Q<TextField>("EditField");
            m_addButton = noteElement.Q<Button>("AddButton");
            m_sendButton = noteElement.Q<Button>("SendButton");
            
            noteElement.Q<Label>("AuthorLabel").text = note.Author;
            noteElement.Q<Label>("CreatedDate").text = note.CreatedDate;
            
            // Set style
            m_contents.style.SetPadding(3, 4, 3, 8);
            m_contents.style.whiteSpace = WhiteSpace.Normal;
            
            // Load tags
            LoadTags();
            
            // Parse text and insert elem
            ParseTextElements(note.NoteContent);

            // register context button event
            m_contextButton.clicked += () =>
            {
                ShowContextMenu(note);
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
            bool isOwnNote = note.Author == UNoteSetting.UserName;
            
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

        #endregion // Constructor
        
        #region Private Method
        
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

        private void LoadTags()
        {
            VisualElement tags = m_noteTag.Q("Tags");
            tags.Clear();
            
            NoteCommentBase comment = m_note as NoteCommentBase;
            if (comment == null)
            {
                return;
            }

            foreach (var id in comment.NoteTagDataIdList)
            {
                tags.Add(new UNoteTag(id, false));
            }
        }
        
        private void EnableEditText()
        {
            m_contents.style.display = DisplayStyle.None;
            m_editNoteElem.style.display = DisplayStyle.Flex;
            m_contextButton.style.display = DisplayStyle.None;

            m_editField.value = m_note.NoteContent;
            m_editField.Focus();
        }

        private void QuitEditText()
        {
            m_contents.style.display = DisplayStyle.Flex;
            m_editNoteElem.style.display = DisplayStyle.None;
            m_contextButton.style.display = DisplayStyle.Flex;
        }

        private void FinishEditText()
        {
            m_contents.style.display = DisplayStyle.Flex;
            m_editNoteElem.style.display = DisplayStyle.None;
            m_contextButton.style.display = DisplayStyle.Flex;

            if (m_note.NoteContent != m_editField.value)
            {
                m_note.NoteContent = m_editField.value;

                // Reset field
                m_contents.Clear();
                ParseTextElements(m_note.NoteContent);
                
                // Update note last updated date
                m_note.UpdatedDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                EditorUNoteManager.SaveAll();
                    
                UNoteEditor.CenterPane?.SetupListItems();
            }
        }
        
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

        private void ParseTextElements(string text)
        {
            const string SplitKey = "[unatt-";
            string[] splitTexts = text.Split(SplitKey, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var splitText in splitTexts)
            {
                // Object ref
                const string objectKey = "guid:";
                if (splitText.StartsWith(objectKey))
                {
                    int startIndex = splitText.IndexOf(objectKey);

                    if (startIndex == -1)
                    {
                        InsertAsLabel(splitText);
                        continue;
                    }

                    int keyLength = objectKey.Length;
                    int endIndex = splitText.IndexOf("]", startIndex + keyLength);
                    if (endIndex == -1)
                    {
                        InsertAsLabel(splitText);
                        continue;
                    }

                    string guid = splitText.Substring(startIndex + keyLength, endIndex - startIndex - keyLength);
                    string restStr = splitText.Substring(endIndex + 1).Trim();
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        InsertAsLabel(splitText);
                        if (!restStr.IsNullOrWhiteSpace())
                        {
                            InsertAsLabel(restStr);   
                        }
                        continue;
                    }

                    // Object field
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                        // Add ObjectField 
                        ObjectField objectField = new ObjectField();
                        objectField.SetEnabled(false);
                        objectField.SetValueWithoutNotify(obj);

                        m_contents.Add(objectField);   
                        
                        // Draw texture
                        if (obj is Texture2D tex)
                        {
                            VisualElement texElem = new FlexibleImageElem(tex, this);
                            texElem.style.SetMargin(2, 0, 2, 0);
                            m_contents.Add(texElem);
                        }
                        
                        if (!restStr.IsNullOrWhiteSpace())
                        {
                            InsertAsLabel(restStr);   
                        }
                        continue;
                    }
                }
                
                // Default
                InsertAsLabel(splitText);

                void InsertAsLabel(string text)
                {
                    Label label = new Label(text.Trim());
                    label.style.SetMargin(2, 0, 2, 0);
                    m_contents.Add(label);
                }
            }
        }

        private string CreateEditedText() => "<size=10><color=#999999> (edited)</color></size>";

        private Label CreateEditedTextElem() => new(CreateEditedText());

        #endregion // Private Method
    }
}
