using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    [Serializable]
    public class NoteInputField : VisualElement
    {
        private NoteType m_bindNoteType;
        private string m_bindId;
        private string m_objectId;
        
        [SerializeField] private NoteEditorModel m_model;
        
        private TextField m_inputText;
        private ScrollView m_inputScroll;
        private Label m_authorLabel;
        private Button m_addTagButton;
        private Button m_addButton;
        private Button m_sendButton;
        
        private float m_lastScrollPosition;
        
        public NoteInputField(NoteType noteType, string bindId, string objectId = null)
        {
            name = nameof(NoteInputField);

            SetNoteInfo(noteType, bindId, objectId);

            m_model = ScriptableObject.CreateInstance<NoteEditorModel>();
            
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteInputField
            );
            contentContainer.Add(tree.Instantiate());
            
            m_inputText = contentContainer.Q<TextField>("InputText");
            m_inputScroll = contentContainer.Q<ScrollView>("TextArea");

            m_authorLabel = contentContainer.Q<Label>("Author");
            m_addTagButton = contentContainer.Q<Button>("AddTagButton");
            m_addButton = contentContainer.Q<Button>("AddButton");
            m_sendButton = contentContainer.Q<Button>("SendButton");
            
            m_authorLabel.text = UNoteSetting.UserName;

            m_addTagButton.Q("Icon").style.backgroundImage 
                = AssetDatabase.LoadAssetAtPath<Texture2D>(PathUtil.GetTexturePath("tag.png"));
            
            m_inputText.BindProperty(m_model.EditingText);
            
            // Prepare drop area
            VisualElementUtil.CreateDropAreaElem(m_inputText);
            
            // show add menu when button clicked
            m_addTagButton.clicked += ShowAddTagMenu;
            m_addButton.clicked += ShowAddMenu;
            
            // create note when button clicked
            m_sendButton.clicked += SendNote;
            
            // scroll text field according to cursor
            EditorApplication.update += () =>
            {
                float pos = m_inputText.cursorPosition.y;
                if (Mathf.Abs(pos - m_lastScrollPosition) > 0.1f)
                {
                    // wait for bounds update
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

            EditorUNoteManager.OnNoteSelected += note =>
            {
                if (note != null)
                {
                    SetNoteInfo(note.NoteType, note.NoteId);   
                }
                
                SetEnabled(note != null);
            };
        }

        public void SetNoteInfo(NoteType noteType, string bindId, string objectId = null)
        {
            m_bindNoteType = noteType;
            m_bindId = bindId;
            m_objectId = objectId;
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return && (evt.ctrlKey || evt.commandKey))
            {
                SendNote();
            }
        }

        void ShowAddTagMenu()
        {
            VisualElement tags = contentContainer.Q("Tags");

            List<UNoteTag> tempList = new();
            tags.Query<UNoteTag>().ToList(tempList);
            
            GenericMenu menu = new GenericMenu();

            List<UNoteTagData> tagList = UNoteSetting.TagList;
            foreach (var tagData in tagList)
            {
                // Overlap check
                if (tempList.Find(t => t.TagId == tagData.TagId) != null)
                {
                    continue;
                }
                
                // Add item
                menu.AddItem(new GUIContent(tagData.TagName), false, () =>
                {
                    tags.Add(new UNoteTag(tagData.TagId, true));
                });
            }
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Open Tag Setting"), false, ()=>
            {
                SettingsService.OpenProjectSettings("Project/UNote");
            });
            
            menu.ShowAsContext();
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
                if (!m_inputText.value.IsNullOrEmpty())
                {
                    m_inputText.value += "\n";
                }
                m_inputText.value += $"[unatt-guid:{guid}]";
            }
        }
        
        private void SendNote()
        {
            if (string.IsNullOrWhiteSpace(m_inputText.value))
            {
                return;
            }

            m_inputText.Unbind();
            
            NoteBase newNoteMessage = null;

            List<string> tagIdList = new List<string>();
            foreach (var noteTag in contentContainer.Q("Tags").Query<UNoteTag>().Build())
            {
                tagIdList.Add(noteTag.TagId);
            }

            switch (m_bindNoteType)
            {
                case NoteType.Project:
                    newNoteMessage = EditorUNoteManager.AddNewProjectNoteMessage(
                        m_bindId,
                        m_inputText.value,
                        new List<string>(tagIdList)
                    );   
                    break;
                
                case NoteType.Asset:
                    // TODO
                    // Add parent note if needed
                    if (m_bindId.IsNullOrEmpty())
                    {
                        AssetNote newNote = EditorUNoteManager.AddNewAssetNote(m_objectId, "");
                        m_bindId = newNote.NoteId;
                    }
                    
                    // Add note message
                    newNoteMessage = EditorUNoteManager.AddNewAssetNoteMessage(
                        m_bindId,
                        m_inputText.value,
                        new List<string>(tagIdList)
                    );
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (newNoteMessage == null)
            {
                return;
            }
            
            contentContainer.Q("Tags").Clear();
            
            // update state before rebind
            m_model.ModelObject.Update();
            m_model.EditingText.stringValue = "";
            m_model.ModelObject.ApplyModifiedProperties();

            m_inputText.BindProperty(m_model.EditingText);
        }
    }
}
