using System;
using System.Collections;
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
        
        [SerializeField]
        private NoteEditorModel m_model;
        
        private TextField m_inputText;
        private ScrollView m_inputScroll;
        private Label m_authorLabel;
        private Button m_sendButton;
        
        private float m_lastScrollPosition;
        
        public NoteInputField(NoteType noteType, string bindId)
        {
            name = nameof(NoteInputField);

            m_bindNoteType = noteType;
            m_bindId = bindId;

            m_model = ScriptableObject.CreateInstance<NoteEditorModel>();
            
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteInputField
            );
            contentContainer.Add(tree.Instantiate());
            
            m_inputText = contentContainer.Q<TextField>("InputText");
            m_inputScroll = contentContainer.Q<ScrollView>("TextArea");

            m_authorLabel = contentContainer.Q<Label>("Author");
            m_sendButton = contentContainer.Q<Button>("SendButton");
            
            m_authorLabel.text = UserConfig.GetUNoteSetting().UserName;
            
            m_inputText.BindProperty(m_model.EditingText);
            
            // ボタンを押したら新しいメモを作成
            m_sendButton.clicked += SendNote;
            
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
                    newLeafNote = EditorUNoteManager.AddLeafAssetNote(
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
            
            // バインドしなおす前に最新の状態にする
            m_model.ModelObject.Update();
            m_model.EditingText.stringValue = "";
            m_model.ModelObject.ApplyModifiedProperties();

            m_inputText.BindProperty(m_model.EditingText);
        }
    }
}
