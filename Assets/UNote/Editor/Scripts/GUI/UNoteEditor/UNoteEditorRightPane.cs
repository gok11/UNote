using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class UNoteEditorRightPane : VisualElement
    {
        public UNoteEditorRightPane()
        {
            name = nameof(UNoteEditorRightPane);

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.UNoteEditorRightPane
            );
            contentContainer.Add(tree.Instantiate());

            if (EditorUNoteManager.CurrentNote != null)
            {
                Debug.Log("ok");
            }
        }
    }
}
