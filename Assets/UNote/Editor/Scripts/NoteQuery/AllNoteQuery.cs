namespace UNote.Editor
{
    public class AllNotesQuery : PresetQuery
    {
        public AllNotesQuery(string searchText = null,
            string[] searchTags = null, bool showArchive = true, bool showFavoriteFirst = true)
            : base("All Notes", searchText, searchTags, showArchive, showFavoriteFirst)
        {
            NoteTypeFilter = NoteTypeFilter.All;
        }
    }
}
