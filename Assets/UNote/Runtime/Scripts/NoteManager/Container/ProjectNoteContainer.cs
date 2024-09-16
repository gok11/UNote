using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region Field
        
        [SerializeField] private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();
        [FormerlySerializedAs("m_projectLeafNoteList")] [SerializeField] private List<ProjectNoteComment> m_projectCommentList = new List<ProjectNoteComment>();
        
        #endregion // Field
        
        public List<ProjectNote> GetProjectNoteList() => m_projectNoteList;

        public List<ProjectNoteComment> GetProjectCommentList() => m_projectCommentList;
    }
}
