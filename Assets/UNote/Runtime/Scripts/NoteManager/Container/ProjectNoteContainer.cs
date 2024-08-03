using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region Field
        
        [SerializeField] private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();
        [SerializeField] private List<ProjectLeafNote> m_projectLeafNoteList = new List<ProjectLeafNote>();
        
        #endregion // Field
        
        public List<ProjectNote> GetProjectNoteList() => m_projectNoteList;

        public List<ProjectLeafNote> GetProjectLeafNoteList() => m_projectLeafNoteList;
    }
}
