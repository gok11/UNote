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

        private Label m_noteCategoryLabel;
        
        private ScrollView m_noteScroll;

        public UNoteEditorCenterPane(UNoteEditor noteEditor)
        {
            name = nameof(UNoteEditorCenterPane);

            m_noteEditor = noteEditor;

            contentContainer.style.minWidth = 160;

            // Instantiate pane
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorCenterPane
            );
            TemplateContainer template = tree.CloneTree();
            contentContainer.Add(template);

            m_noteCategoryLabel = template.Q<Label>("NoteCategoryLabel");
            
            m_noteScroll = template.Q<ScrollView>("NoteList");

            // Setup
            SetupListItems();

            // Handle mouse event
            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    ShowContextMenu(evt.mousePosition);
                }
            });
            
            // Register note event
            EditorUNoteManager.OnNoteAdded += note => SetupListItems();
            EditorUNoteManager.OnNoteSelected += note => UpdateNoteList();
            EditorUNoteManager.OnNoteDeleted += note => SetupListItems();
        }

        private void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("New Note"),
                false,
                () =>
                {
                    NoteBase newNote = null;
                    
                    // メモ追加
                    switch (EditorUNoteManager.CurrentNoteType)
                    {
                        case NoteType.Project:
                            newNote = EditorUNoteManager.AddNewProjectNote();
                            break;
                        
                        case NoteType.Asset:
                            AssetNoteAddWindow addWindow = ScriptableObject.CreateInstance<AssetNoteAddWindow>();
                            addWindow.ShowUtility();
                            break;
                        
                        default:
                            throw new NotImplementedException();
                    }

                    if (newNote != null)
                    {
                        EditorUNoteManager.SelectNote(newNote);
                    }
                }
            );
            menu.ShowAsContext();
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
                    m_noteCategoryLabel.text = "Project Note";
                    notes = EditorUNoteManager.GetAllProjectNotes();
                    break;

                case NoteType.Asset:
                    m_noteCategoryLabel.text = "Asset Note";
                    notes = EditorUNoteManager.GetAllAssetNotes();
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
            // TODO 検索内容に応じてメモを絞る

            // 背景色を更新
            foreach (var item in m_noteScroll.contentContainer.Query<UNoteEditorListItem>().Build())
            {
                item.SelectItem(item.BindNote == EditorUNoteManager.CurrentNote);
            }
        }

        public override void OnUndoRedo(string undoRedoName)
        {
            SetupListItems();
        }
    }
}
