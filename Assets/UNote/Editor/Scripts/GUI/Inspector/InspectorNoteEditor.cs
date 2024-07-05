using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class InspectorNoteEditor : VisualElement
    {
        public InspectorNoteEditor(NoteType noteType, Object target)
        {
            name = nameof(InspectorNoteEditor);

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.InspectorNoteEditor
            );
            contentContainer.Add(tree.Instantiate());
        }
    }
}
