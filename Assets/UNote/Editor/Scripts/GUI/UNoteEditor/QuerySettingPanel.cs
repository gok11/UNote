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

            SetQuery(noteQuery);
            
            // Set style
            m_noteType.Q<Label>().style.minWidth = 90;
            m_noteSort.Q<Label>().style.minWidth = 90;
            m_displayArchive.Q<Label>().style.minWidth = 90;
            
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
        }

        internal void SetQuery(NoteQuery noteQuery)
        {
            m_noteQuery = noteQuery;

            m_searchText.SetValueWithoutNotify(noteQuery.SearchText);
            m_noteType.SetValueWithoutNotify(noteQuery.NoteTypeFilter);
            m_noteSort.SetValueWithoutNotify(noteQuery.NoteQuerySort);
            m_displayArchive.SetValueWithoutNotify(noteQuery.DisplayArchive);
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
