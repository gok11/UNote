using System;

namespace UNote.Editor
{
    [Serializable]
    public class ProjectNotesQuery : PresetQuery
    {
        #region Constructor

        public ProjectNotesQuery()
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "Project Notes";
            NoteTypeFilter = NoteTypeFilter.Project;   
        }
        
        public ProjectNotesQuery(string searchText = null, string[] searchTags = null, bool displayArchive = false) : this()
        {
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }

        #endregion // Constructor
    }
}
