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
        public SerializedObject ModelObject => m_modelObject ??= new SerializedObject(this);
    }

    public class UNoteEditor : EditorWindow
    {
        [SerializeField]
        private NoteEditorModel m_model;

        #region Property

        public UNoteEditorLeftPane LeftPane { get; private set; }

        public UNoteEditorCenterPane CenterPane { get; private set; }

        public UNoteEditorRightPane RightPane { get; private set; }

        public NoteEditorModel Model => m_model;

        #endregion // Property

        [MenuItem("UNote/Note Editor")]
        private static void OpenWindow()
        {
            GetWindow<UNoteEditor>("UNote Editor");
        }

        private void CreateGUI()
        {
            m_model = CreateInstance<NoteEditorModel>();

            VisualElement root = rootVisualElement;

            // Three split pane
            int fixedPaneIndex = 0;
            float leftPaneInitialWidth = 150f;
            float centerPaneInitialWidth = 200f;
            TwoPaneSplitViewOrientation orientation = TwoPaneSplitViewOrientation.Horizontal;

            TwoPaneSplitView firstSplitView = new TwoPaneSplitView(
                fixedPaneIndex,
                leftPaneInitialWidth,
                orientation
            );
            TwoPaneSplitView secondSplitView = new TwoPaneSplitView(
                fixedPaneIndex,
                centerPaneInitialWidth,
                orientation
            );
            root.Add(firstSplitView);

            LeftPane = new UNoteEditorLeftPane(this);
            LeftPane.style.minWidth = leftPaneInitialWidth;
            CenterPane = new UNoteEditorCenterPane(this);
            CenterPane.style.minWidth = centerPaneInitialWidth;
            RightPane = new UNoteEditorRightPane(this);

            firstSplitView.Add(LeftPane);
            firstSplitView.Add(secondSplitView);

            secondSplitView.Add(CenterPane);
            secondSplitView.Add(RightPane);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath.UNoteEditor);
            root.styleSheets.Add(styleSheet);

            // Undo Setup
            Undo.undoRedoEvent += (in UndoRedoInfo info) =>
            {
                if (info.undoName.Contains("UNote"))
                {
                    LeftPane.OnUndoRedo();
                    CenterPane.OnUndoRedo();
                    RightPane.OnUndoRedo();
                }
            };
        }
    }
}
