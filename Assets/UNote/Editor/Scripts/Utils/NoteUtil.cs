using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public static class NoteUtil
    {
        internal static bool IsFavorite(this NoteBase note)
        {
            return EditorUNoteManager.GetFavoriteNoteList().Contains(note.NoteId);
        }
    }
}
