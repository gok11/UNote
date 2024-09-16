using System;

namespace UNote.Editor
{
    [Serializable]
    public class ProjectNotesQuery : PresetQuery
    {
        #region Constructor

        public ProjectNotesQuery(string searchText = null,
            string[] searchTags = null, bool showArchive = true, bool showFavoriteFirst = true)
            : base("Project Notes", searchText, searchTags, showArchive, showFavoriteFirst)
        {
            NoteTypeFilter = NoteTypeFilter.Project;
        }

        #endregion // Constructor
    }
}
