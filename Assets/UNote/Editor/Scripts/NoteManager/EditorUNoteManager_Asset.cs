using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private List<AssetNote> m_assetNoteList = new();
        private List<AssetLeafNote> m_assetLeafNoteList = new();

        private Dictionary<string, AssetNote> m_assetNoteDict = new();
        private Dictionary<string, List<AssetLeafNote>> m_assetNoteDictByTitle = new();

        #endregion // Field

        private static IEnumerable<AssetNote> GetAssetNoteAll() => Instance.m_assetNoteDict.Values;

        public static List<AssetLeafNote> GetAssetLeafNoteListByAssetNoteId(string assetNoteId)
        {
            if (Instance.m_assetNoteDictByTitle.TryGetValue(assetNoteId, out var leafNoteList))
            {
                return leafNoteList;
            }

            List<AssetLeafNote> newList = new(64);

            foreach (var note in Instance.m_assetLeafNoteList)
            {
                if (note.NoteId == assetNoteId)
                {
                    newList.Add(note);
                }
            }

            Instance.m_assetNoteDictByTitle.Add(assetNoteId, newList);
            return newList;
        }

        public static void ClearAssetCache()
        {
            Instance.m_assetNoteList.Clear();
            Instance.m_assetLeafNoteList.Clear();
            Instance.m_assetNoteDict.Clear();
            Instance.m_assetNoteDictByTitle.Clear();
        }
    }
}
