using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public abstract class PresetQuery : NoteQuery
    {
        protected PresetQuery(string queryName, string searchText, string[] searchTags, bool showArchive, bool showFavoriteFirst)
            : base(queryName, searchText, searchTags, showArchive, showFavoriteFirst) { }

        internal override bool IsOverWritable => false;
    }
}
