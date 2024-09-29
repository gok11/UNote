using System;
using System.Collections.Generic;
using System.Linq;
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
        private VisualElement m_customQueryElem;

        private NoteQuery m_currentQuery;
        
        private Dictionary<NoteQuery, VisualElement> m_presetQueryElemDict = new();
        private Dictionary<NoteQuery, VisualElement> m_customQueryElemDict = new();

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
            m_customQueryElem = paneContainer.Q("QueryElems");

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
            m_presetQueryElemDict.Add(initQuery, allNoteElem);
            m_presetQueryElemDict.Add(new ProjectNotesQuery(), projectNoteElem);
            m_presetQueryElemDict.Add(new AssetNotesQuery(), assetNoteElem);

            EditorUNoteManager.SetNoteQuery(initQuery);
            SelectQueryElem(initQuery);

            // Register select event
            foreach (var pair in m_presetQueryElemDict)
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
            Button addQueryButton = paneContainer.Q<Button>("AddQueryButton");
            
            LoadCustomQuery();

            addQueryButton.clicked += () =>
            {
                NoteQuery newQuery = EditorUNoteManager.AddQuery();
                
                NoteQuery cloneQuery = newQuery.Clone();
                CustomNoteQueryElem queryElem = new CustomNoteQueryElem(this, cloneQuery);
                m_customQueryElem.Add(queryElem);
                m_customQueryElemDict.Add(cloneQuery, queryElem);
                
                EditorUNoteManager.SetNoteQuery(cloneQuery);
                UNoteEditor.CenterPane.EnableChangeQueryNameMode();
            };
            
            m_noteAddButton.clicked += ShowAddWindow;
            
            // Query event
            EditorUNoteManager.OnNoteQuerySelected += query =>
            {
                UpdateElemBackgroundColor(query);
            };

            EditorUNoteManager.OnNoteQueryDeleted += _ =>
            {
                LoadCustomQuery();

                // Select other query
                CustomQueryContainer container = CustomQueryContainer.Get();
                if (container.NoteQueryList.Count > 0)
                {
                    EditorUNoteManager.SetNoteQuery(container.NoteQueryList[0]);
                }
                else
                {
                    SetDefaultQuery();
                }
            };
        }

        #endregion // Constructor

        #region Internal Method

        internal void SetDefaultQuery()
        {
            NoteQuery query = m_presetQueryElemDict.Keys.First();
            EditorUNoteManager.SetNoteQuery(query);
        }
        
        internal void LoadCustomQuery()
        {
            m_customQueryElem.Clear();
            m_customQueryElemDict.Clear();
            
            List<NoteQuery> queryList = CustomQueryContainer.Get().NoteQueryList;
            
            foreach (var query in queryList)
            {
                NoteQuery cloneQuery = query.Clone();
                CustomNoteQueryElem queryElem = new CustomNoteQueryElem(this, cloneQuery);
                m_customQueryElem.Add(queryElem);
                m_customQueryElemDict.Add(cloneQuery, queryElem);
            }
        }

        internal void SelectQueryElem(NoteQuery noteQuery)
        {
            UpdateElemBackgroundColor(noteQuery);
            
            // Select internal
            if (noteQuery == m_currentQuery)
            {
                return;
            }
            
            UNoteEditor.CenterPane?.SetupListItems();
            m_currentQuery = noteQuery;
        }

        internal void UpdateElemBackgroundColor(NoteQuery noteQuery)
        {
            // Set background color
            foreach (var queryElem in m_presetQueryElemDict)
            {
                m_presetQueryElemDict[queryElem.Key].contentContainer.style.backgroundColor =
                    noteQuery.QueryID == queryElem.Key.QueryID ? StyleUtil.SelectColor : StyleUtil.UnselectColor;
            }
            
            foreach (var queryElem in m_customQueryElemDict)
            {
                m_customQueryElemDict[queryElem.Key].contentContainer.style.backgroundColor =
                    noteQuery.QueryID == queryElem.Key.QueryID ? StyleUtil.SelectColor : StyleUtil.UnselectColor;
            }
        }
        
        #endregion // Internal Method

        #region Private Method
        
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
