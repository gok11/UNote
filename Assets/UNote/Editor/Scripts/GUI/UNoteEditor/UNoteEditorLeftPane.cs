using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class UNoteEditorLeftPane : VisualElement
    {
        public UNoteEditorLeftPane()
        {
            name = nameof(UNoteEditorLeftPane);

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorLeftPane
            );
            contentContainer.Add(tree.Instantiate());
        }

        /// <summary>
        /// Toggle fold state
        /// </summary>
        public void ToggleFoldState() { }
    }
}
