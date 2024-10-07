using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    /// <summary>
    /// NoteEditor
    /// </summary>
    public class UNoteEditor : EditorWindow
    {
        [SerializeField] private NoteEditorModel m_model;
        
        internal static UNoteEditorLeftPane LeftPane { get; private set; }

        internal static UNoteEditorCenterPane CenterPane { get; private set; }

        internal static UNoteEditorRightPane RightPane { get; private set; }

        public NoteEditorModel Model => m_model;

        [MenuItem("UNote/Note Editor")]
        private static void OpenWindow()
        {
            GetWindow<UNoteEditor>("UNote Editor");
        }

        private void CreateGUI()
        {
            // wait for initialize
            if (!Utility.Initialized)
            {
                EditorApplication.delayCall += CreateGUI;
                return;
            }
            
            // create gui impl
            m_model = CreateInstance<NoteEditorModel>();

            VisualElement root = rootVisualElement;

            // Three split pane
            int fixedPaneIndex = 0;
            float leftPaneInitialWidth = 140f;
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

            // Undo event
            Undo.undoRedoEvent += (in UndoRedoInfo info) =>
            {
                LeftPane.OnUndoRedo(info.undoName);
                CenterPane.OnUndoRedo(info.undoName);
                RightPane.OnUndoRedo(info.undoName);
                
                EditorUNoteManager.SaveAll();
            };
        }
    }
}
