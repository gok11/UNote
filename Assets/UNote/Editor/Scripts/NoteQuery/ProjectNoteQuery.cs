using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

namespace UNote.Editor
{
    [Serializable]
    public class ProjectNotesQuery : NoteQuery
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
