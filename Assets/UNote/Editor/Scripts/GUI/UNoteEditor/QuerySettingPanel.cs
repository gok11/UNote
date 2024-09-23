using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
            m_displayArchive = contentContainer.Q<Toggle>("DisplayArchive");

            m_saveQueryButton = contentContainer.Q<Button>("SaveButton");
            m_deleteQueryButton = contentContainer.Q<Button>("DeleteButton");

            SetQuery(noteQuery);
            
            // Set style
            m_noteType.Q<Label>().style.minWidth = 90;
            m_noteSort.Q<Label>().style.minWidth = 90;
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
                CustomQueryContainer.Get().Save();
            };

            m_deleteQueryButton.clicked += () =>
            {
                CustomQueryContainer container = CustomQueryContainer.Get();
                container.NoteQueryList.Remove(m_noteQuery);

                UNoteEditorLeftPane leftPane = UNoteEditor.LeftPane;
                leftPane.LoadCustomQuery();
                
                if (container.NoteQueryList.Count > 0)
                {
                    EditorUNoteManager.SetNoteQuery(container.NoteQueryList[0]);
                    leftPane.UpdateElemBackgroundColor(container.NoteQueryList[0]);
                }
                else
                {
                    leftPane.SetDefaultQuery();
                }
                
                container.Save();
            };
        }

        internal void SetQuery(NoteQuery noteQuery)
        {
            m_noteQuery = noteQuery;

            m_searchText.SetValueWithoutNotify(noteQuery.SearchText);
            m_noteType.SetValueWithoutNotify(noteQuery.NoteTypeFilter);
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
