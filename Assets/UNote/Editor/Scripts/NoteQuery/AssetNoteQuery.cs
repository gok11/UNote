using System;

namespace UNote.Editor
{
    public class AssetNotesQuery : PresetQuery
    {
        #region Constructor

        public AssetNotesQuery()
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "Asset Notes";
            NoteTypeFilter = NoteTypeFilter.Asset;   
        }
        
        public AssetNotesQuery(string searchText = null, string[] searchTags = null, bool displayArchive = false) : this()
        {
            SearchText = searchText;
            SearchTags = searchTags;
            DisplayArchive = displayArchive;
        }

        #endregion // Constructor
    }
}
