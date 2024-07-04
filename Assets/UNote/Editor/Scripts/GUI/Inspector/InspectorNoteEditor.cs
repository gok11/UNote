using System.Collections;
using System.Collections.Generic;
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
        }
    }
}
