using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// NoteEditor part VisualElement. Note list for current query.
    /// </summary>
    public class UNoteEditorCenterPane : UNoteEditorPaneBase
    {
        private UNoteEditor m_noteEditor;

        private Label m_noteQueryLabel;
        private TextField m_noteQueryField;
        
        private QuerySettingPanel m_querySettingPanel;
        private Button m_openSettingPanelButton;
        private ScrollView m_noteScroll;

        internal UNoteEditorCenterPane(UNoteEditor noteEditor)
        {
            name = nameof(UNoteEditorCenterPane);

            m_noteEditor = noteEditor;

            contentContainer.style.minWidth = 160;

            // Instantiate pane
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteEditorCenterPane
            );
            TemplateContainer template = tree.CloneTree();
            contentContainer.Add(template);

            m_noteQueryLabel = template.Q<Label>("NoteQueryLabel");
            m_noteQueryField = template.Q<TextField>("NoteQueryField");
            
            m_openSettingPanelButton = template.Q<Button>("SettingPanelButton");
            m_noteScroll = template.Q<ScrollView>("NoteList");
            
            // Insert query panel under query label
            m_querySettingPanel = new QuerySettingPanel(EditorUNoteManager.CurrentNoteQuery);
            template.Insert(1, m_querySettingPanel);

            // Setup
            EditorApplication.delayCall += SetupListItems;
            
            // setup button
            m_openSettingPanelButton.Q("Image").style.backgroundImage =
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathUtil.GetTexturePath("setting.png"));

            m_openSettingPanelButton.clicked += () =>
            {
                m_querySettingPanel.ToggleDisplay();
            };
            
            // Register text event
            m_noteQueryLabel.RegisterCallback<MouseDownEvent>(_ =>
            {
                EnableChangeQueryNameMode();
            });
            
            m_noteQueryField.RegisterCallback<BlurEvent>(_ =>
            {
                SetNameGUIEditMode(false);
            });
            
            // Register note event
            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 1)
                {
                    return;
                }
                
                GenericMenu menu = new GenericMenu();
                
                menu.AddItem(new GUIContent("Add Note"), false, () =>
                {
                    if (EditorWindow.HasOpenInstances<NoteAddWindow>())
                    {
                        return;
                    }
            
                    NoteAddWindow addWindow = ScriptableObject.CreateInstance<NoteAddWindow>();
                    addWindow.ShowUtility(); 
                });
                
                menu.ShowAsContext();
            });
            
            // Register note event
            EditorUNoteManager.OnNoteAdded += _ => SetupListItems();
            EditorUNoteManager.OnNoteSelected += n =>
            {
                if (n != null)
                {
                    UpdateNoteBackground();
                }
            };
            EditorUNoteManager.OnNoteDeleted += _ => SetupListItems();
            EditorUNoteManager.OnNoteQuerySelected += query =>
            {
                m_querySettingPanel.SetQuery(query);
                SetupListItems();
            };
            EditorUNoteManager.OnNoteFavoriteChanged += _ => SetupListItems();
        }

        /// <summary>
        /// Enable NoteQuery edit mode.
        /// </summary>
        internal void EnableChangeQueryNameMode()
        {
            NoteQuery noteQuery = EditorUNoteManager.CurrentNoteQuery;
            
            if (noteQuery == null)
            {
                return;
            }

            if (!noteQuery.IsOverWritable)
            {
                return;
            }

            SetNameGUIEditMode(true);

            m_noteQueryField.value = noteQuery.QueryName;
            m_noteQueryField.Focus();

            m_noteQueryField.UnregisterCallback<KeyDownEvent>(TryChangeName);
            m_noteQueryField.RegisterCallback<KeyDownEvent>(TryChangeName);
            
            // Change mode
            void TryChangeName(KeyDownEvent evt)
            {
                // Save query name
                if (evt.keyCode == KeyCode.Return)
                {
                    // except invalid value
                    if (m_noteQueryField.value.IsNullOrWhiteSpace())
                    {
                        SetNameGUIEditMode(false);
                        m_noteQueryField.UnregisterCallback<KeyDownEvent>(TryChangeName);
                        return;
                    }
                    
                    // exec rename
                    noteQuery.QueryName = m_noteQueryField.value;
                    
                    CustomQueryContainer container = CustomQueryContainer.Get();
                    int sourceIndex = container.NoteQueryList.FindIndex(t => t.QueryID == noteQuery.QueryID);
                    if (sourceIndex >= 0)
                    {
                        container.NoteQueryList[sourceIndex] = noteQuery;
                        CustomQueryContainer.Get().Save();   
                    }

                    m_noteQueryLabel.text = noteQuery.QueryName;
                    UNoteEditor.LeftPane.LoadCustomQuery();
                    
                    m_noteQueryField.UnregisterCallback<KeyDownEvent>(TryChangeName);
                }

                if (evt.keyCode == KeyCode.Escape)
                {
                    SetNameGUIEditMode(false);
                    
                    m_noteQueryField.UnregisterCallback<KeyDownEvent>(TryChangeName);
                }
            }
        }
        
        /// <summary>
        /// Load note list
        /// </summary>
        internal void SetupListItems()
        {
            NoteQuery noteQuery = EditorUNoteManager.CurrentNoteQuery;
            
            if (noteQuery == null)
            {
                return;
            }
            
            VisualElement container = m_noteScroll.contentContainer;
            container.Clear();
            
            // Prepare item
            m_noteQueryLabel.text = noteQuery.QueryName;
            IEnumerable<NoteBase> notes = EditorUNoteManager.GetFilteredNotes(noteQuery);
            
            // Sort
            notes = EditorUNoteManager.SortNotes(notes, noteQuery);
            
            bool existCurrentNote = false;
            
            // Add content to list
            foreach (var note in notes)
            {
                UNoteEditorListItem item = new UNoteEditorListItem(m_noteEditor);
                container.Add(item);
                item.Setup(note);

                existCurrentNote |= note == EditorUNoteManager.CurrentNote;
            }

            if (!existCurrentNote)
            {
                // Select first found note
                NoteBase firstNote = notes.FirstOrDefault();
                EditorUNoteManager.SelectNote(firstNote);      
            }

            UpdateNoteBackground();
        }

        internal override void OnUndoRedo(string undoRedoName)
        {
            SetupListItems();
        }

        /// <summary>
        /// Enable note edit mode.
        /// </summary>
        /// <param name="enableEdit"></param>
        private void SetNameGUIEditMode(bool enableEdit)
        {
            if (enableEdit)
            {
                m_noteQueryLabel.style.display = DisplayStyle.None;
                m_noteQueryField.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_noteQueryLabel.style.display = DisplayStyle.Flex;
                m_noteQueryField.style.display = DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Update background color of each element
        /// </summary>
        private void UpdateNoteBackground()
        {
            if (EditorUNoteManager.CurrentNote == null)
            {
                return;
            }
            
            // Update background color
            UQueryState<UNoteEditorListItem> items = m_noteScroll.contentContainer.Query<UNoteEditorListItem>().Build();
            
            foreach (var item in items)
            {
                item.SetBackgroundColor(item.BindNote == EditorUNoteManager.CurrentNote);
            }
        }
    }
}
