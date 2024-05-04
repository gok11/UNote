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

            // Instantiate pane
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorCenterPane
            );
            TemplateContainer template = tree.CloneTree();
            contentContainer.Add(template);

            // Prepare item
            IReadOnlyList<ProjectNote> noteList = RuntimeUNoteManager.GetProjectNoteList();

            // Add content to list
            ScrollView scrollView = template.Q<ScrollView>("NoteList");
            VisualElement container = scrollView.contentContainer;

            foreach (var note in noteList)
            {
                UNoteEditorListItem item = new UNoteEditorListItem();
                item.Setup(note, container);
            }
        }

        public void FilterNotesBySearchText() { }
    }
}
