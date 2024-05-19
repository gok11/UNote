using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorCenterPane : UNoteEditorPaneBase
    {
        private ScrollView m_noteScroll;

        public UNoteEditorCenterPane(NoteEditor noteEditor)
        {
            name = nameof(UNoteEditorCenterPane);

            contentContainer.style.minWidth = 160;

            // Instantiate pane
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorCenterPane
            );
            TemplateContainer template = tree.CloneTree();
            contentContainer.Add(template);

            // Prepare item
            IEnumerable<NoteBase> notes = null;

            switch (EditorUNoteManager.CurrentNoteType)
            {
                case NoteType.Project:
                    notes = EditorUNoteManager.GetAllProjectNotes();
                    break;
            }

            // Add content to list
            m_noteScroll = template.Q<ScrollView>("NoteList");
            VisualElement container = m_noteScroll.contentContainer;

            foreach (var note in notes)
            {
                UNoteEditorListItem item = new UNoteEditorListItem(noteEditor);
                container.Add(item);
                item.Setup(note);
            }

            UpdateNoteList();

            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(
                        new GUIContent("New Note"),
                        false,
                        () =>
                        {
                            // メモ追加
                            EditorUNoteManager.CreateProjectNoteContainerObject();
                            ProjectNote newNote = EditorUNoteManager.AddNewProjectNote();
                            EditorUNoteManager.SelectRoot(newNote);

                            // ビューに反映
                            UNoteEditorListItem newItem = new UNoteEditorListItem(noteEditor);
                            container.Add(newItem);
                            newItem.Setup(newNote);
                        }
                    );
                    menu.ShowAsContext();
                }
            });
        }

        public void UpdateNoteList()
        {
            // TODO 検索内容に応じてメモを絞る

            // 背景色を更新
            foreach (var item in m_noteScroll.contentContainer.Query<UNoteEditorListItem>().Build())
            {
                item.SelectItem(item.BindNote == EditorUNoteManager.CurrentRootNote);
            }
        }
    }
}
