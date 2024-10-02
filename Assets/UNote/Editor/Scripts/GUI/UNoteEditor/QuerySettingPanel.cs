using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    
    public class QuerySettingPanel : VisualElement
    {
        #region Field

        private NoteQuery m_noteQuery;

        private TextField m_searchText;
        private EnumField m_noteType;
        private EnumField m_noteSort;
        private EnumFlagsField m_noteTag;
        private Toggle m_displayArchive;

        private Button m_saveQueryButton;
        private Button m_deleteQueryButton;
        
        #endregion // Field

        internal QuerySettingPanel(NoteQuery noteQuery)
        {
            name = nameof(QuerySettingPanel);

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteEditorQuerySettingPanel
            );
            contentContainer.Add(tree.CloneTree());
            contentContainer.style.display = DisplayStyle.None;

            m_searchText = contentContainer.Q<TextField>("SearchField");
            m_noteType = contentContainer.Q<EnumField>("NoteType");
            m_noteSort = contentContainer.Q<EnumField>("NoteSort");
            m_noteTag = contentContainer.Q<EnumFlagsField>("NoteTag");
            m_displayArchive = contentContainer.Q<Toggle>("DisplayArchive");

            m_saveQueryButton = contentContainer.Q<Button>("SaveButton");
            m_deleteQueryButton = contentContainer.Q<Button>("DeleteButton");
            
            // Create note tag field
            int insertIndex = m_noteSort.parent.IndexOf(m_noteSort);
            m_noteTag = new EnumFlagsField("Note Tag", NoteTags.All);
            m_noteSort.parent.Insert(insertIndex, m_noteTag);
            
            SetQuery(noteQuery);
            
            // Set style
            m_noteType.Q<Label>().style.minWidth = 90;
            m_noteSort.Q<Label>().style.minWidth = 90;
            m_noteTag.Q<Label>().style.minWidth = 90;
            m_noteTag.Q<Label>().style.fontSize = 11;
            m_displayArchive.Q<Label>().style.minWidth = 90;

            VisualElement deleteIcon = m_deleteQueryButton.Q("Icon"); 
            deleteIcon.style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>(PathUtil.GetTexturePath("trash.png"));
            deleteIcon.style.unityBackgroundImageTintColor = StyleUtil.GrayColor;
            
            // Register callback
            m_searchText.RegisterValueChangedCallback(x =>
            {
                m_noteQuery.SearchText = x.newValue;
                EditorUNoteManager.CallUpdateNoteQuery();
            });
            m_noteType.RegisterValueChangedCallback(x =>
            {
                m_noteQuery.NoteTypeFilter = (NoteTypeFilter)x.newValue;
                EditorUNoteManager.CallUpdateNoteQuery();
            });
            m_noteTag.RegisterValueChangedCallback(x =>
            {
                m_noteQuery.SearchTags = (NoteTags)x.newValue;
                EditorUNoteManager.CallUpdateNoteQuery();
            });
            m_noteSort.RegisterValueChangedCallback(x =>
            {
                m_noteQuery.NoteQuerySort = (NoteQuerySort)x.newValue;
                EditorUNoteManager.CallUpdateNoteQuery();
            });
            m_displayArchive.RegisterValueChangedCallback(x =>
            {
                m_noteQuery.DisplayArchive = x.newValue;
                EditorUNoteManager.CallUpdateNoteQuery();
            });

            m_saveQueryButton.clicked += () =>
            {
                CustomQueryContainer container = CustomQueryContainer.Get();
                int sourceIndex = container.NoteQueryList.FindIndex(t => t.QueryID == m_noteQuery.QueryID);
                if (sourceIndex >= 0)
                {
                    container.NoteQueryList[sourceIndex] = m_noteQuery;
                    CustomQueryContainer.Get().Save();   
                }
            };

            m_deleteQueryButton.clicked += () =>
            {
                CustomQueryContainer container = CustomQueryContainer.Get();
                int sourceIndex = container.NoteQueryList.FindIndex(t => t.QueryID == m_noteQuery.QueryID);
                if (sourceIndex >= 0)
                {
                    container.NoteQueryList.RemoveAt(sourceIndex);   
                }

                UNoteEditorLeftPane leftPane = UNoteEditor.LeftPane;
                leftPane.LoadCustomQuery();
                
                if (container.NoteQueryList.Count > 0)
                {
                    EditorUNoteManager.SetNoteQuery(container.NoteQueryList[0]);
                }
                else
                {
                    leftPane.SetDefaultQuery();
                }
                
                container.Save();
            };
        }

        // private void UpdateQueryTags(NoteTags tags)
        // {
        //     List<UNoteTagData> tagDataList = UNoteSetting.TagList;
        //
        //     foreach (NoteTags noteTag in Enum.GetValues(typeof(NoteTags)))
        //     {
        //         if (noteTag == NoteTags.None || noteTag == NoteTags.All)
        //         {
        //             continue;
        //         }
        //
        //         UNoteTagData tagData = tagDataList.Find(t => t.TagName == noteTag.ToString());
        //         if (tagData == null)
        //         {
        //             continue;
        //         }
        //             
        //         bool isOn = (tags & noteTag) != 0;
        //         if (isOn)
        //         {
        //             if (!m_noteQuery.SearchTagList.Contains(tagData.TagId))
        //             {
        //                 Debug.Log($"Add: {tagData.TagName}");
        //                 m_noteQuery.SearchTagList.Add(tagData.TagId);   
        //             }
        //         }
        //         else
        //         {
        //             if (m_noteQuery.SearchTagList.Contains(tagData.TagId))
        //             {
        //                 Debug.Log($"Remove: {tagData.TagName}");
        //                 m_noteQuery.SearchTagList.Remove(tagData.TagId);   
        //             }
        //         }
        //     }
        // }

        /// <summary>
        /// Set new query
        /// </summary>
        /// <param name="noteQuery"></param>
        internal void SetQuery(NoteQuery noteQuery)
        {
            m_noteQuery = noteQuery;

            // Avoid view update after each field update
            m_searchText.SetValueWithoutNotify(noteQuery.SearchText);
            m_noteType.SetValueWithoutNotify(noteQuery.NoteTypeFilter);
            m_noteTag.SetValueWithoutNotify(noteQuery.SearchTags);
            m_noteSort.SetValueWithoutNotify(noteQuery.NoteQuerySort);
            m_displayArchive.SetValueWithoutNotify(noteQuery.DisplayArchive);

            bool isNotPreset = noteQuery.IsOverWritable;
            m_noteType.SetEnabled(isNotPreset);
            m_saveQueryButton.style.display = isNotPreset ? DisplayStyle.Flex : DisplayStyle.None;
            m_deleteQueryButton.style.display = isNotPreset ? DisplayStyle.Flex : DisplayStyle.None;
        }

        internal void ToggleDisplay()
        {
            contentContainer.style.display =
                contentContainer.style.display == DisplayStyle.Flex
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
        }
    }
}
