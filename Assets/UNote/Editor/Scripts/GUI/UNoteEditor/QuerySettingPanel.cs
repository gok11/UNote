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

        internal delegate void QueryUpdateHandler(NoteQuery noteQuery);

        internal static event QueryUpdateHandler OnQueryUpdated;
        
        #endregion // Field
        
        internal QuerySettingPanel(NoteQuery noteQuery)
        {
            name = nameof(QuerySettingPanel);

            m_noteQuery = noteQuery;

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteEditorQuerySettingPanel
            );
            contentContainer.Add(tree.CloneTree());
            contentContainer.style.display = DisplayStyle.None;
            
            // Set style
            contentContainer.Q<EnumField>("NoteType").Q<Label>().style.minWidth = 90;
            contentContainer.Q<EnumField>("NoteSort").Q<Label>().style.minWidth = 90;
            contentContainer.Q<Toggle>("DisplayArchive").Q<Label>().style.minWidth = 90;
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
