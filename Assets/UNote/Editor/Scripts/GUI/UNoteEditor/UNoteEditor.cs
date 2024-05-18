using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public partial class NoteEditorModel : ScriptableObject
    {
        private SerializedObject m_modelObject;
        public SerializedObject ModelObject
        {
            get
            {
                if (m_modelObject == null)
                {
                    m_modelObject = new SerializedObject(this);
                }
                return m_modelObject;
            }
        }
    }

    public class NoteEditor : EditorWindow
    {
        [SerializeField]
        private NoteEditorModel m_model;

        public NoteEditorModel Model => m_model;

        [MenuItem("UNote/Note Editor")]
        private static void OpenWindow()
        {
            GetWindow<NoteEditor>("Note Editor");
        }

        private void CreateGUI()
        {
            m_model = CreateInstance<NoteEditorModel>();

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

            UNoteEditorLeftPane leftPane = new UNoteEditorLeftPane(this);
            UNoteEditorCenterPane centerPane = new UNoteEditorCenterPane(this);
            UNoteEditorRightPane rightPane = new UNoteEditorRightPane(this);

            firstSplitView.Add(leftPane);
            firstSplitView.Add(secondSplitView);

            secondSplitView.Add(centerPane);
            secondSplitView.Add(rightPane);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath.UNoteEditor);
            root.styleSheets.Add(styleSheet);

            // Undo Setup
            Undo.undoRedoEvent += (in UndoRedoInfo info) =>
            {
                if (info.undoName.Contains("UNote"))
                {
                    leftPane.OnUndoRedo();
                    centerPane.OnUndoRedo();
                    rightPane.OnUndoRedo();
                }
            };
        }
    }
}
