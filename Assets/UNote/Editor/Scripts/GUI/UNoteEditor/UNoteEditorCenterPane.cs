using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorCenterPane : VisualElement
    {
        public UNoteEditorCenterPane()
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
            IReadOnlyList<NoteBase> noteList = null;

            switch (EditorUNoteManager.CurrentNoteType)
            {
                case NoteType.Project:
                    noteList = EditorUNoteManager.GetOwnProjectNoteList();
                    break;
            }

            // Add content to list
            ScrollView scrollView = template.Q<ScrollView>("NoteList");
            VisualElement container = scrollView.contentContainer;

            foreach (var note in noteList)
            {
                UNoteEditorListItem item = new UNoteEditorListItem();
                container.Add(item);
                item.Setup(note);
            }

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
                            ProjectNote newNote = EditorUNoteManager.AddNewProjectNote();
                            UNoteEditorListItem newItem = new UNoteEditorListItem();
                            container.Add(newItem);
                            newItem.Setup(newNote);
                            EditorUNoteManager.Select(newNote);
                        }
                    );
                    menu.ShowAsContext();
                }
            });
        }

        public void FilterNotesBySearchText() { }
    }
}
