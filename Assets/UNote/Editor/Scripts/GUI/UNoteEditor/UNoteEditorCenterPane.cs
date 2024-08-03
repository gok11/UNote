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
            EditorUNoteManager.OnNoteSelected += _ => UpdateNoteList();
            EditorUNoteManager.OnNoteDeleted += _ => SetupListItems();
        }

        public void SetupListItems()
        {
            VisualElement container = m_noteScroll.contentContainer;
            container.Clear();

            // Prepare item
            IEnumerable<NoteBase> notes = null;

            switch (EditorUNoteManager.CurrentNoteType)
            {
                case NoteType.Project:
                    m_noteQueryLabel.text = "Project Note";
                    notes = EditorUNoteManager.GetProjectNoteAllList().OrderBy(t => t.CreatedDate);
                    break;

                case NoteType.Asset:
                    m_noteQueryLabel.text = "Asset Note";
                    notes = EditorUNoteManager.GetAllAssetNotesIdDistinct().OrderBy(t => t.CreatedDate);
                    break;
                
                default:
                    throw new NotImplementedException();
            }

            // Add content to list
            foreach (var note in notes)
            {
                UNoteEditorListItem item = new UNoteEditorListItem(m_noteEditor);
                container.Add(item);
                item.Setup(note);
            }

            UpdateNoteList();
        }

        public void UpdateNoteList()
        {
            // TODO filter notes

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
