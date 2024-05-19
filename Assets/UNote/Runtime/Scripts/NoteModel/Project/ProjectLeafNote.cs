using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public class ProjectLeafNote : NoteBase
    {
        public override NoteType NoteType => NoteType.Project;

        // ID上書き
        public ProjectLeafNote(string guid)
            : base()
        {
            base.m_noteId = guid;
        }
    }
}
