using System;
using System.Collections.Generic;

namespace UNote.Editor
{
    public class AssetNotesQuery : PresetQuery
    {
        public AssetNotesQuery()
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "Asset Notes";
            SearchTags = NoteTags.All;
            NoteTypeFilter = NoteTypeFilter.Asset;   
        }
        
        public AssetNotesQuery(string searchText = null, NoteTags searchTags = NoteTags.All, bool displayArchive = false) : this()
        {
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }
    }
}
