using System;
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
        #region Field

        private NoteType m_bindNoteType;
        private string m_bindId;
        private string m_objectId;
        
        [SerializeField] private NoteEditorModel m_model;
        
        private TextField m_inputText;
        private ScrollView m_inputScroll;
        private Label m_authorLabel;
        private Button m_sendButton;
        
        private float m_lastScrollPosition;

        #endregion // Field

        #region Constructor

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
            m_sendButton = contentContainer.Q<Button>("SendButton");
            
            m_authorLabel.text = UNoteSetting.UserName;
            
            m_inputText.BindProperty(m_model.EditingText);
            
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

        #endregion // Constructor

        #region Public Methods

        public void SetNoteInfo(NoteType noteType, string bindId, string objectId = null)
        {
            m_bindNoteType = noteType;
            m_bindId = bindId;
            m_objectId = objectId;
        }

        #endregion Public Methods

        #region Private Methods

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return && (evt.ctrlKey || evt.commandKey))
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

            m_inputText.Unbind();
            
            NoteBase newLeafNote = null;

            switch (m_bindNoteType)
            {
                case NoteType.Project:
                    newLeafNote = EditorUNoteManager.AddNewLeafProjectNote(
                        m_bindId,
                        m_inputText.value
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
                    
                    // Add leaf note
                    newLeafNote = EditorUNoteManager.AddNewAssetLeafNote(
                        m_bindId,
                        m_inputText.value
                    );
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (newLeafNote == null)
            {
                return;
            }
            
            // update state before rebind
            m_model.ModelObject.Update();
            m_model.EditingText.stringValue = "";
            m_model.ModelObject.ApplyModifiedProperties();

            m_inputText.BindProperty(m_model.EditingText);
        }

        #endregion // Private Methods
    }
}
