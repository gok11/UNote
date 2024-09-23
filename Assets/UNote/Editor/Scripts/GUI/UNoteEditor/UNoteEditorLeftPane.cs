using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

            VisualElement presetViewElem = paneContainer.Q("PresetQueries");

            m_noteAddButton = paneContainer.Q<Button>("AddNoteButton");

            paneContainer.Q("Spacer").style.flexGrow = 1;
            
            // Category
            VisualTreeAsset categoryTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteTypeItem
            );
            TemplateContainer categoryContainer = categoryTree.Instantiate();
            presetViewElem.Add(categoryContainer);

            // Init query elem
            VisualElement allNoteElem = categoryContainer.Q("AllNoteElem");
            VisualElement projectNoteElem = categoryContainer.Q("ProjectNoteElem");
            VisualElement assetNoteElem = categoryContainer.Q("AssetNoteElem");
            
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
                    NoteQuery query;

                    switch (pair.Key.NoteTypeFilter)
                    {
                        case NoteTypeFilter.All:
                            query = pair.Key.Clone<AllNotesQuery>();
                            break;
                        
                        case NoteTypeFilter.Project:
                            query = pair.Key.Clone<ProjectNotesQuery>();
                            break;
                        
                        case NoteTypeFilter.Asset:
                            query = pair.Key.Clone<AssetNotesQuery>();
                            break;
                        
                        default:
                            throw new NotImplementedException();
                    }
                    
                    EditorUNoteManager.SetNoteQuery(query);
                    SelectQueryElem(query);
                    evt.StopPropagation();
                });
            }
            
            // Load custom query
            VisualElement customQueryElem = paneContainer.Q("CustomQueries");
            Button addQueryButton = paneContainer.Q<Button>("AddQueryButton");
            
            LoadCustomQuery(customQueryElem);

            addQueryButton.clicked += () =>
            {
                NoteQuery newQuery = new NoteQuery();
                CustomQueryContainer.Get().NoteQueryList.Add(newQuery);

                NoteQueryTemplate queryElem = new NoteQueryTemplate(this, newQuery);
                customQueryElem.Add(queryElem);
                m_queryElemDict.Add(newQuery, queryElem);
                
                CustomQueryContainer.Get().Save();
            };
            
            m_noteAddButton.clicked += ShowAddWindow;
        }

        #endregion // Constructor

        #region Private Method
        
        private void LoadCustomQuery(VisualElement customQueryElem)
        {
            List<NoteQuery> queryList = CustomQueryContainer.Get().NoteQueryList;
            
            foreach (var query in queryList)
            {
                NoteQueryTemplate queryElem = new NoteQueryTemplate(this, query);
                customQueryElem.Add(queryElem);
                m_queryElemDict.Add(query, queryElem);
            }
        }

        internal void SelectQueryElem(NoteQuery noteQuery)
        {
            // Set background color
            foreach (var queryElem in m_queryElemDict)
            {
                m_queryElemDict[queryElem.Key].contentContainer.style.backgroundColor =
                    noteQuery.QueryID == queryElem.Key.QueryID ? StyleUtil.SelectColor : StyleUtil.UnselectColor;
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
