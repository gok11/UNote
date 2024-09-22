using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorCenterPane : UNoteEditorPaneBase
    {
        #region Field

        private UNoteEditor m_noteEditor;

        private Label m_noteQueryLabel;
        private QuerySettingPanel m_querySettingPanel;
        private Button m_openSettingPanelButton;
        private ScrollView m_noteScroll;

        #endregion // Field

        #region Constructor

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

            m_noteQueryLabel = template.Q<Label>("NoteCategoryLabel");
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
            EditorUNoteManager.OnNoteQueryUpdated += query =>
            {
                m_querySettingPanel.SetQuery(query);
                SetupListItems();
            };
            EditorUNoteManager.OnNoteFavoriteChanged += _ => SetupListItems();
        }

        #endregion // Constructor

        #region Internal Method

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
            switch (noteQuery.NoteQuerySort)
            {
                case NoteQuerySort.UpdateDate:
                    notes = notes.OrderByDescending(t => t.UpdatedDate);
                    break;
                case NoteQuerySort.UpdateDateAscending:
                    notes = notes.OrderBy(t => t.UpdatedDate);
                    break;
                case NoteQuerySort.CreateDate:
                    notes = notes.OrderByDescending(t => t.CreatedDate);
                    break;
                case NoteQuerySort.CreateDateAscending:
                    notes = notes.OrderBy(t => t.CreatedDate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            notes = notes.OrderByDescending(t => t.IsFavorite());
            
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

        #endregion

        #region Private Method

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

        #endregion // Private Method
    }
}
