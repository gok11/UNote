using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private List<AssetNote> m_assetNoteList = new();
        private List<AssetLeafNote> m_assetLeafNoteList = new();

        private Dictionary<string, List<AssetNote>> m_assetNoteDict = new();
        private Dictionary<string, List<AssetLeafNote>> m_assetLeafNoteDict = new();

        #endregion // Field

        private static IReadOnlyList<AssetNote> GetAssetNoteAllList() => Instance.m_assetNoteList;
        private static IReadOnlyList<AssetLeafNote> GetAssetLeafNoteAllList() => Instance.m_assetLeafNoteList;

        public static List<AssetNote> GetAssetNoteListByGUID(string guid)
        {
            if (Instance.m_assetNoteDict.TryGetValue(guid, out var noteList))
            {
                return noteList;
            }
            
            List<AssetNote> newList = new List<AssetNote>(64);
            
            foreach (var note in Instance.m_assetNoteList)
            {
                if (note.NoteId == guid)
                {
                    newList.Add(note);
                }
            }
            Instance.m_assetNoteDict.Add(guid, newList);

            return newList;
        }

        public static IEnumerable<AssetNote> GetAllAssetNotesIdDistinct()
        {
            List<AssetNote> tempList = new List<AssetNote>();
            
            foreach (var key in Instance.m_assetNoteDict.Keys)
            {
                AssetNote note = GetAssetNoteListByGUID(key).FirstOrDefault();
                if (note != null)
                {
                    tempList.Add(note);   
                }
            }

            return tempList;
        }

        public static List<AssetLeafNote> GetAssetLeafNoteListByNoteId(string assetNoteId)
        {
            if (Instance.m_assetLeafNoteDict.TryGetValue(assetNoteId, out var leafNoteList))
            {
                return leafNoteList;
            }

            List<AssetLeafNote> newList = new List<AssetLeafNote>(64);
            
            foreach (var note in Instance.m_assetLeafNoteList)
            {
                if (note.ReferenceNoteId == assetNoteId)
                {
                    newList.Add(note);
                }
            }
            Instance.m_assetLeafNoteDict.Add(assetNoteId, newList);

            return newList;
        }

        public static void ClearAssetCache()
        {
            Instance.m_assetNoteList.Clear();
            Instance.m_assetLeafNoteList.Clear();
            Instance.m_assetNoteDict.Clear();
            Instance.m_assetLeafNoteDict.Clear();
        }
    }
}
