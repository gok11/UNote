using System;
using System.Collections.Generic;

namespace UNote.Editor
{
    public class AllNotesQuery : PresetQuery
    {
        public AllNotesQuery()
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "All Notes";
            SearchTags = NoteTags.All;
            NoteTypeFilter = NoteTypeFilter.All;            
        }
        
        public AllNotesQuery(string searchText = null, NoteTags searchTags = NoteTags.All, bool displayArchive = false) : this()
        {
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }
    }
}
