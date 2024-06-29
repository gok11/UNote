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

        public UNoteEditorLeftPane(UNoteEditor noteEditor)
        {
            name = nameof(UNoteEditorLeftPane);

            m_noteEditor = noteEditor;

            style.backgroundColor = new Color(0.17f, 0.17f, 0.17f);
            
            // Pane
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorLeftPane
            );
            TemplateContainer paneContainer = tree.Instantiate();
            contentContainer.Add(paneContainer);

            // Category
            VisualTreeAsset categoryTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteTypeItem
            );
            TemplateContainer categoryContainer = categoryTree.Instantiate();
            paneContainer.Add(categoryContainer);

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
        }

        private void SelectCategoryElem(NoteType noteType)
        {
            foreach (var category in m_categoryElemDict)
            {
                m_categoryElemDict[category.Key].contentContainer.style.backgroundColor =
                    noteType == category.Key ? StyleUtil.SelectColor : StyleUtil.UnselectColor;
            }
            
            m_noteEditor.CenterPane?.SetupListItems();
            EditorUNoteManager.SelectCategory(noteType);
        }

        /// <summary>
        /// Toggle fold state
        /// </summary>
        public void ToggleFoldState() { }
    }
}
