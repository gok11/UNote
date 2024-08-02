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
        private Dictionary<string, List<ProjectLeafNote>> m_projectNoteDictByGUID = new();
        
        #endregion
        
        internal static IReadOnlyList<ProjectNote> GetProjectNoteAllList() => Instance.m_projectNoteList;
        internal static IReadOnlyList<ProjectLeafNote> GetProjectLeafNoteAllList() => Instance.m_projectLeafNoteList;
        
        public static List<ProjectLeafNote> GetProjectLeafNoteListByNoteId(string projectNoteId)
        {
            if (Instance.m_projectNoteDictByGUID.TryGetValue(projectNoteId, out var leafNoteList))
            {
                return leafNoteList;
            }

            List<ProjectLeafNote> newList = new(64);

            foreach (var note in Instance.m_projectLeafNoteList)
            {
                if (note.ReferenceNoteId == projectNoteId)
                {
                    newList.Add(note);
                }
            }

            Instance.m_projectNoteDictByGUID.Add(projectNoteId, newList);
            return newList;
        }

        public static void ClearProjectCache()
        {
            Instance.m_projectNoteList.Clear();
            Instance.m_projectLeafNoteList.Clear();
            Instance.m_projectNoteDict.Clear();
            Instance.m_projectNoteDictByGUID.Clear();
        }
    }
}
