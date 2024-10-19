using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public class SceneNoteMessage : NoteMessageBase
    {
        public SceneNoteSubInfoType subInfoType = SceneNoteSubInfoType.None;
        
        public override NoteType NoteType => NoteType.Scene;
    }
}
