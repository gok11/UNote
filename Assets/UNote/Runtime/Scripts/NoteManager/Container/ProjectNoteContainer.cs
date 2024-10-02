using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region Field
        
        [SerializeField] private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();
        [FormerlySerializedAs("m_projectCommentList")] [FormerlySerializedAs("m_projectLeafNoteList")] [SerializeField] private List<ProjectNoteMessage> m_projectMessageList = new List<ProjectNoteMessage>();
        
        #endregion // Field
        
        public List<ProjectNote> GetProjectNoteList() => m_projectNoteList;

        public List<ProjectNoteMessage> GetProjectMessageList() => m_projectMessageList;
    }
}
