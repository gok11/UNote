using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public class AssetNoteQuery : NoteQuery
    {
        public AssetNoteQuery(string queryName, string searchText,
            string[] searchTags, bool showArchive, bool showFavoriteFirst)
            : base(queryName, searchText, searchTags, showArchive, showFavoriteFirst)
        {
            QueryName = "Asset Note";
            NoteTypeFilter = NoteTypeFilter.Asset;
        }
    }
}
