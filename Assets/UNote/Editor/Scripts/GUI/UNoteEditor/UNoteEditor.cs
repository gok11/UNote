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

            // Three split pane
            int fixedPaneIndex = 0;
            float leftPaneInitialWidth = 150f;
            float middlePaneInitialWidth = 200f;
            TwoPaneSplitViewOrientation orientation = TwoPaneSplitViewOrientation.Horizontal;

            TwoPaneSplitView firstSplitView = new TwoPaneSplitView(
                fixedPaneIndex,
                leftPaneInitialWidth,
                orientation
            );
            TwoPaneSplitView secondSplitView = new TwoPaneSplitView(
                fixedPaneIndex,
                middlePaneInitialWidth,
                orientation
            );
            root.Add(firstSplitView);

            // TODO: get from dedicated class
            VisualElement leftPane = new UNoteEditorLeftPane();
            VisualElement middlePane = new VisualElement();
            VisualElement rightPane = new VisualElement();

            firstSplitView.Add(leftPane);
            firstSplitView.Add(secondSplitView);

            secondSplitView.Add(middlePane);
            secondSplitView.Add(rightPane);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath.UNoteEditor);
            root.styleSheets.Add(styleSheet);
        }
    }
}
