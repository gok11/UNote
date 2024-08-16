using UnityEngine.UIElements;

namespace UNote.Editor
{
    public abstract class UNoteEditorPaneBase : VisualElement
    {
        internal virtual void OnUndoRedo(string undoName) { }
    }
}
