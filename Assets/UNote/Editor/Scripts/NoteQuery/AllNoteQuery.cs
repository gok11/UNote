using System;

namespace UNote.Editor
{
    public class AllNotesQuery : PresetQuery
    {
        public AllNotesQuery(string searchText = null, string[] searchTags = null, bool displayArchive = false)
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "All Notes";
            NoteTypeFilter = NoteTypeFilter.All;
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }
    }
}
