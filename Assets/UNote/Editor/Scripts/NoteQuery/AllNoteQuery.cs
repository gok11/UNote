using System;

namespace UNote.Editor
{
    public class AllNotesQuery : PresetQuery
    {
        public AllNotesQuery()
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "All Notes";
            NoteTypeFilter = NoteTypeFilter.All;            
        }
        
        public AllNotesQuery(string searchText = null, string[] searchTags = null, bool displayArchive = false) : this()
        {
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }
    }
}
