using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public abstract class PresetQuery : NoteQuery
    {
        internal override bool IsOverWritable => false;
    }
}
