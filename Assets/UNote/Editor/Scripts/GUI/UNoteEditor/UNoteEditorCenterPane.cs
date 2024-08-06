using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using NotImplementedException = System.NotImplementedException;
using PopupWindow = UnityEditor.PopupWindow;

namespace UNote.Editor
{
    public class UNoteEditorCenterPane : UNoteEditorPaneBase
    {
        private UNoteEditor m_noteEditor;

        private Label m_noteQueryLabel;
        
        private ScrollView m_noteScroll;

        public UNoteEditorCenterPane(UNoteEditor noteEditor)
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
            
            m_noteScroll = template.Q<ScrollView>("NoteList");

            // Setup
            SetupListItems();
            
            // Register note event
            EditorUNoteManager.OnNoteAdded += _ => SetupListItems();
            EditorUNoteManager.OnNoteSelected += _ => UpdateNoteBackground();
            EditorUNoteManager.OnNoteDeleted += _ => SetupListItems();
            EditorUNoteManager.OnNoteQueryUpdated += _ => SetupListItems();
        }

        public void SetupListItems()
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
            
            // TODO favorite first setting
            
            notes = notes.OrderBy(t => t.CreatedDate);

            // Add content to list
            foreach (var note in notes)
            {
                UNoteEditorListItem item = new UNoteEditorListItem(m_noteEditor);
                container.Add(item);
                item.Setup(note);
            }

            UpdateNoteBackground();
        }

        private void UpdateNoteBackground()
        {
            // Update background color
            foreach (var item in m_noteScroll.contentContainer.Query<UNoteEditorListItem>().Build())
            {
                item.SetBackgroundColor(item.BindNote == EditorUNoteManager.CurrentNote);
            }
        }

        public override void OnUndoRedo(string undoRedoName)
        {
            SetupListItems();
        }
    }
}
