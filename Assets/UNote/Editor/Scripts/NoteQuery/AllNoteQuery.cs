using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public class AllNotesQuery : NoteQuery
    {
        public AllNotesQuery(string searchText = null,
            string[] searchTags = null, bool showArchive = true, bool showFavoriteFirst = true)
            : base("All Notes", searchText, searchTags, showArchive, showFavoriteFirst)
        {
            NoteTypeFilter = NoteTypeFilter.All;
        }
    }
}
