using System;
using System.Collections.Generic;

namespace UNote.Editor
{
    [Serializable]
    public class ProjectNotesQuery : PresetQuery
    {
        public ProjectNotesQuery()
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "Project Notes";
            SearchTags = NoteTags.All;
            NoteTypeFilter = NoteTypeFilter.Project;   
        }
        
        public ProjectNotesQuery(string searchText = null, NoteTags searchTags = NoteTags.All, bool displayArchive = false) : this()
        {
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }
    }
}
