using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public class SceneNoteQuery : PresetQuery
    {
        public SceneNoteQuery()
        {
            QueryID = Guid.NewGuid().ToString();
            QueryName = "Scene Notes";
            SearchTags = NoteTags.All;
            NoteTypeFilter = NoteTypeFilter.Scene;
        }
    }
}
