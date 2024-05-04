using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

            // Add content to list
            ScrollView scrollView = template.Q<ScrollView>("NoteList");
            VisualElement container = scrollView.contentContainer;

            VisualTreeAsset listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteListItem
            );

            for (int i = 0; i < 5; i++)
            {
                container.Add(listItem.CloneTree());
            }
        }

        public void FilterNotesBySearchText() { }
    }
}
