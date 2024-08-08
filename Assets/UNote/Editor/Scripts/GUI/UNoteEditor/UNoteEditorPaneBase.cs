using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public abstract class UNoteEditorPaneBase : VisualElement
    {
        internal virtual void OnUndoRedo(string undoName) { }
    }
}
