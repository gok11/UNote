using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorLeftPane : UNoteEditorPaneBase
    {
        private UNoteEditor m_noteEditor;
        
        private Dictionary<NoteType, VisualElement> m_categoryElemDict = new();

        private NoteType m_currentNoteType = NoteType.Project;

        public UNoteEditorLeftPane(UNoteEditor noteEditor)
        {
            name = nameof(UNoteEditorLeftPane);

            m_noteEditor = noteEditor;

            style.backgroundColor = new Color(0.17f, 0.17f, 0.17f);
            
            // Pane
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteEditorLeftPane
            );
            TemplateContainer paneContainer = tree.Instantiate();
            contentContainer.Add(paneContainer);

            VisualElement presetViewElem = paneContainer.Q("PresetViews");

            // Category
            VisualTreeAsset categoryTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteTypeItem
            );
            TemplateContainer categoryContainer = categoryTree.Instantiate();
            presetViewElem.Add(categoryContainer);

            VisualElement projectNoteElem = categoryContainer.Q("ProjectNoteElem");
            VisualElement assetNoteElem = categoryContainer.Q("AssetNoteElem");

            // Update color
            m_categoryElemDict.Add(NoteType.Project, projectNoteElem);
            m_categoryElemDict.Add(NoteType.Asset, assetNoteElem);

            SelectCategoryElem(EditorUNoteManager.CurrentNoteType);

            // Register select event
            foreach (var category in m_categoryElemDict)
            {
                category.Value.RegisterCallback<MouseDownEvent>(evt =>
                {
                    EditorUNoteManager.SelectCategory(category.Key);
                    SelectCategoryElem(category.Key);
                });
            }

            EditorUNoteManager.OnNoteSelected += note =>
            {
                if (note != null)
                {
                    SelectCategoryElem(note.NoteType);   
                }
            };
        }

        private void SelectCategoryElem(NoteType noteType)
        {
            // Set background color
            foreach (var category in m_categoryElemDict)
            {
                m_categoryElemDict[category.Key].contentContainer.style.backgroundColor =
                    noteType == category.Key ? StyleUtil.SelectColor : StyleUtil.UnselectColor;
            }
            
            // Select internal
            if (noteType == m_currentNoteType)
            {
                return;
            }
            
            m_noteEditor.CenterPane?.SetupListItems();
            m_currentNoteType = noteType;
        }
    }
}
