using System.Collections.Generic;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region Field
        
        [SerializeField] private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();
        [SerializeField] private List<ProjectNoteComment> m_projectLeafNoteList = new List<ProjectNoteComment>();
        
        #endregion // Field
        
        public List<ProjectNote> GetProjectNoteList() => m_projectNoteList;

        public List<ProjectNoteComment> GetProjectLeafNoteList() => m_projectLeafNoteList;
    }
}
