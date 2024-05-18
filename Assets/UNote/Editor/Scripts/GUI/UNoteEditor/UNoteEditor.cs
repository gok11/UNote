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

        private UNoteEditorLeftPane m_leftPane;
        private UNoteEditorCenterPane m_centerPane;
        private UNoteEditorRightPane m_rightPane;

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

            m_leftPane = new UNoteEditorLeftPane(this);
            m_centerPane = new UNoteEditorCenterPane(this);
            m_rightPane = new UNoteEditorRightPane(this);

            firstSplitView.Add(m_leftPane);
            firstSplitView.Add(secondSplitView);

            secondSplitView.Add(m_centerPane);
            secondSplitView.Add(m_rightPane);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath.UNoteEditor);
            root.styleSheets.Add(styleSheet);

            // Undo Setup
            Undo.undoRedoEvent += (in UndoRedoInfo info) =>
            {
                if (info.undoName.Contains("UNote"))
                {
                    m_leftPane.OnUndoRedo();
                    m_centerPane.OnUndoRedo();
                    m_rightPane.OnUndoRedo();
                }
            };
        }
    }
}
