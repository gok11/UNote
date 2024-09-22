using System;

namespace UNote.Editor
{
    public class AssetNotesQuery : PresetQuery
    {
        #region Constructor

        public AssetNotesQuery(string searchText = null, string[] searchTags = null, bool displayArchive = false)
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "Asset Notes";
            NoteTypeFilter = NoteTypeFilter.Asset;
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }

        #endregion // Constructor
    }
}
