using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public class ProjectNoteQuery : NoteQuery
    {
        public ProjectNoteQuery(string queryName, string searchText,
            string[] searchTags, bool showArchive, bool showFavoriteFirst)
            : base(queryName, searchText, searchTags, showArchive, showFavoriteFirst)
        {
            QueryName = "Project Note";
            NoteTypeFilter = NoteTypeFilter.Project;
        }
    }
}
