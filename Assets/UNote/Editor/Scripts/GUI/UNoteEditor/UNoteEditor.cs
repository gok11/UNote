using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class NoteEditor : EditorWindow
    {
        [MenuItem("UNote/Note Editor")]
        private static void OpenWindow()
        {
            GetWindow<NoteEditor>("Note Editor");
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.UNoteEditor);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath.UNoteEditor);

            visualTree.CloneTree(root);
            root.styleSheets.Add(styleSheet);
        }
    }
}
