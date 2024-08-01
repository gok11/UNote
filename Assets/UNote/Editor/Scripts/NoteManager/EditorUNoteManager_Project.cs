using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private List<ProjectNote> m_projectNoteList = null;
        private List<ProjectLeafNote> m_projectLeafNoteList = null;

        private Dictionary<string, ProjectNote> m_projectNoteDict = new();
        private Dictionary<string, List<ProjectLeafNote>> m_projectNoteDictByTitle = new();
        
        #endregion
        
        private static IEnumerable<ProjectNote> GetProjectNoteAll() => Instance.m_projectNoteDict.Values;
        
        public static List<ProjectLeafNote> GetProjectLeafNoteListByProjectNoteId(string projectNoteId)
        {
            if (Instance.m_projectNoteDictByTitle.TryGetValue(projectNoteId, out var leafNoteList))
            {
                return leafNoteList;
            }

            List<ProjectLeafNote> newList = new(64);

            foreach (var note in Instance.m_projectLeafNoteList)
            {
                if (note.NoteId == projectNoteId)
                {
                    newList.Add(note);
                }
            }

            Instance.m_projectNoteDictByTitle.Add(projectNoteId, newList);
            return newList;
        }

        public static void ClearProjectCache()
        {
            Instance.m_projectNoteList.Clear();
            Instance.m_projectLeafNoteList.Clear();
            Instance.m_projectNoteDict.Clear();
            Instance.m_projectNoteDictByTitle.Clear();
        }
    }
}
