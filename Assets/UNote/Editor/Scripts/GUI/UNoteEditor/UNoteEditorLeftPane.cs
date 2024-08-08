using System;
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
        #region Field

        private UNoteEditor m_noteEditor;
        private Button m_noteAddButton;

        private NoteQuery m_currentQuery;
        
        private Dictionary<NoteQuery, VisualElement> m_queryElemDict = new();

        #endregion // Field

        #region Constructor

                internal UNoteEditorLeftPane(UNoteEditor noteEditor)
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
            
            paneContainer.StretchToParentSize();

            VisualElement presetViewElem = paneContainer.Q("PresetViews");

            m_noteAddButton = paneContainer.Q<Button>("AddNoteButton");

            paneContainer.Q("Spacer").style.flexGrow = 1;
            
            // Category
            VisualTreeAsset categoryTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteTypeItem
            );
            TemplateContainer categoryContainer = categoryTree.Instantiate();
            presetViewElem.Add(categoryContainer);

            VisualElement allNoteElem = categoryContainer.Q("AllNoteElem");
            VisualElement projectNoteElem = categoryContainer.Q("ProjectNoteElem");
            VisualElement assetNoteElem = categoryContainer.Q("AssetNoteElem");

            // Update color
            AllNotesQuery initQuery = new AllNotesQuery();
            m_queryElemDict.Add(initQuery, allNoteElem);
            m_queryElemDict.Add(new ProjectNotesQuery(), projectNoteElem);
            m_queryElemDict.Add(new AssetNotesQuery(), assetNoteElem);

            SelectQueryElem(initQuery);

            // Register select event
            foreach (var pair in m_queryElemDict)
            {
                pair.Value.RegisterCallback<MouseDownEvent>(evt =>
                {
                    EditorUNoteManager.SetNoteQuery(pair.Key);
                    SelectQueryElem(pair.Key);
                    evt.StopPropagation();
                });
            }
            
            m_noteAddButton.clicked += ShowAddWindow;
        }

        #endregion // Constructor

        #region Private Method

        private void SelectQueryElem(NoteQuery noteQuery)
        {
            // Set background color
            foreach (var category in m_queryElemDict)
            {
                m_queryElemDict[category.Key].contentContainer.style.backgroundColor =
                    noteQuery == category.Key ? StyleUtil.SelectColor : StyleUtil.UnselectColor;
            }
            
            // Select internal
            if (noteQuery == m_currentQuery)
            {
                return;
            }
            
            m_noteEditor.CenterPane?.SetupListItems();
            m_currentQuery = noteQuery;
        }
        
        private void ShowAddWindow()
        {
            if (EditorWindow.HasOpenInstances<NoteAddWindow>())
            {
                return;
            }
            
            NoteAddWindow addWindow = ScriptableObject.CreateInstance<NoteAddWindow>();
            addWindow.ShowUtility();
        }

        #endregion // Private Method
    }
}
