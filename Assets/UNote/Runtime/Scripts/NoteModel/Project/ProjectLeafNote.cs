using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public sealed class ProjectLeafNote : NoteBase
    {
        public override NoteType NoteType => NoteType.Project;
    }
}
