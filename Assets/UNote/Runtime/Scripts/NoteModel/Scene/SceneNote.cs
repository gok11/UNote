using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public class SceneNote : RootNoteBase
    {
        public override NoteType NoteType => NoteType.Scene;
        
#if UNITY_EDITOR
        protected string m_sceneGuid;
        public string SceneGuid => m_sceneGuid;
#endif

        // TODO: For runtime reference
        // protected string m_sceneName;
    }
}
