using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class FavoriteNoteContainer : NoteContainerBase
    {
        [SerializeField] private List<string> m_favoriteNoteIdList = new ();

        internal List<string> GetFavoriteNoteList() => m_favoriteNoteIdList;
    }
}
