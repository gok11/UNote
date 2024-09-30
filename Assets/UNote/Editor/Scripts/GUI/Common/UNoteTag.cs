using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
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

        public UNoteTag(string tagId)
        {
            name = nameof(UNoteTag);

            TagId = tagId;

            List<UNoteTagData> tagDataList = UNoteSetting.TagList;
            UNoteTagData tagData = tagDataList.Find(t => t.TagId == tagId);
            if (tagData == null)
            {
                return;
            }
            
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteTag
            );
            contentContainer.Add(tree.Instantiate());

            m_tagBackground = contentContainer.Q<VisualElement>("TagBackground");
            m_tagLabel = contentContainer.Q<Label>("TagLabel");

            m_tagBackground.style.backgroundColor = tagData.Color;
            m_tagLabel.text = tagData.TagName;
            
            RegisterCallback<MouseDownEvent>(evt =>
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
}
