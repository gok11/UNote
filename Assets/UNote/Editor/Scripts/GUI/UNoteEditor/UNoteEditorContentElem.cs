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

        // private Label m_contentText;
        private VisualElement m_contents;
        private VisualElement m_editNoteElem;
        private Button m_contextButton;
        
        private TextField m_editField;
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

            m_contents = noteElement.Q("Contents");
            m_editNoteElem = noteElement.Q<VisualElement>("EditNoteElem");
            m_contextButton = noteElement.Q<Button>("ContextButton");
            
            m_editField = noteElement.Q<TextField>("EditField");
            m_sendButton = noteElement.Q<Button>("SendButton");
            
            noteElement.Q<Label>("AuthorLabel").text = note.Author;
            noteElement.Q<Label>("CreatedDate").text = note.CreatedDate;
            
            // Set style
            m_contents.style.SetPadding(3, 4, 3, 8);
            m_contents.style.whiteSpace = WhiteSpace.Normal;
            
            // Parse text and insert elem
            ParseTextElements(note.NoteContent);

            // register context button event
            m_contextButton.clicked += () =>
            {
                ShowContextMenu(note);
            };
            
            // edit event
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

        private void ParseTextElements(string text)
        {
            const string SplitKey = "[unref-";
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

                        // Draw texture
                        if (obj is Texture2D tex)
                        {
                            VisualElement texElem = CreateTextureElement(tex, this);
                            texElem.style.SetMargin(2, 0, 2, 0);
                            m_contents.Add(texElem);
                        }
                        
                        // Add ObjectField 
                        ObjectField objectField = new ObjectField();
                        objectField.SetEnabled(false);
                        objectField.SetValueWithoutNotify(obj);

                        m_contents.Add(objectField);   
                        
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

        private VisualElement CreateTextureElement(Texture2D tex, VisualElement parent)
        {
            // float maxWidth = parent.style.width.value.value;
            float maxWidth = tex.width;

            float width = tex.width;
            float height = tex.height;
            
            if (width > maxWidth)
            {
                float ratio = maxWidth / width;

                width = maxWidth;
                height *= ratio;
            }
            
            VisualElement texElem = new VisualElement();
            texElem.style.backgroundImage = tex;
            texElem.style.width = width;
            texElem.style.height = height;
            return texElem;
        }

        #endregion // Private Method
    }
}
