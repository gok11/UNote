using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public class AssetNotesQuery : NoteQuery
    {
        public AssetNotesQuery(string searchText = null, string[] searchTags = null,
            bool showArchive = true, bool showFavoriteFirst = true)
            : base("Asset Notes", searchText, searchTags, showArchive, showFavoriteFirst)
        {
            NoteTypeFilter = NoteTypeFilter.Asset;
        }
    }
}
