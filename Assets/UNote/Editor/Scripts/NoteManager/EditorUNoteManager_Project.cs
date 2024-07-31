using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private static List<ProjectNote> s_projectNoteList = new();
        private static List<ProjectLeafNote> s_projectLeafNoteList = new();

        private static Dictionary<string, ProjectNote> s_projectNoteDict = new();
        private static Dictionary<string, List<ProjectLeafNote>> s_projectNoteDictByTitle = new();
        
        #endregion
        
        private static IEnumerable<ProjectNote> GetProjectNoteListAll() => s_projectNoteDict.Values;
        
        public static List<ProjectLeafNote> GetProjectLeafNoteListByProjectNoteId(string projectNoteId)
        {
            if (s_projectNoteDictByTitle.TryGetValue(projectNoteId, out var leafNoteList))
            {
                return leafNoteList;
            }

            List<ProjectLeafNote> newList = new(64);

            foreach (var note in s_projectLeafNoteList)
            {
                if (note.NoteId == projectNoteId)
                {
                    newList.Add(note);
                }
            }

            s_projectNoteDictByTitle.Add(projectNoteId, newList);
            return newList;
        }

        public static void ClearProjectCache()
        {
            s_projectNoteList.Clear();
            s_projectLeafNoteList.Clear();
            s_projectNoteDict.Clear();
            s_projectNoteDictByTitle.Clear();
        }
    }
}
