using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    /// <summary>
    /// Colored note tag element
    /// </summary>
    public class UNoteTag : VisualElement
    {
        private VisualElement m_tagBackground;
        private Label m_tagLabel;

        private string tagId;

        public string TagId
        {
            get => tagId;
            set => tagId = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public UNoteTag(string tagId, bool ifRegisterEvent)
        {
            name = nameof(UNoteTag);

            TagId = tagId;

            List<UNoteTagData> tagDataList = UNoteSetting.TagList;
            UNoteTagData tagData = tagDataList.Find(t => t.TagId == tagId);
            if (tagData == null)
            {
                return;
            }
            
            // UXML, Style
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteTag
            );
            contentContainer.Add(tree.Instantiate());

            m_tagBackground = contentContainer.Q<VisualElement>("TagBackground");
            m_tagLabel = contentContainer.Q<Label>("TagLabel");

            m_tagBackground.style.backgroundColor = tagData.Color;
            m_tagLabel.text = tagData.TagName;

            m_tagLabel.style.color = CalculateBrightness(tagData.Color) > 0.5f
                ? StyleUtil.BlackTextColor
                : StyleUtil.WhiteTextColor;

            // Mouse event
            if (ifRegisterEvent)
            {
                RegisterCallback<MouseDownEvent>(_ =>
                {
                    GenericMenu menu = new GenericMenu();
                
                    menu.AddItem(new GUIContent("Remove"), false, () =>
                    {
                        parent.Remove(this);
                    });
                
                    menu.ShowAsContext();
                });
            }
        }
        
        /// <summary>
        /// Calculate specified color brightness
        /// </summary>
        private float CalculateBrightness(Color color)
        {
            return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
        }
    }
}
