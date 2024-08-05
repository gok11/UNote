using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public class AllNoteQuery : NoteQuery
    {
        public AllNoteQuery(string queryName, string searchText,
            string[] searchTags, bool showArchive, bool showFavoriteFirst)
            : base(queryName, searchText, searchTags, showArchive, showFavoriteFirst)
        {
            NoteTypeFilter = NoteTypeFilter.All;
        }
    }
}
