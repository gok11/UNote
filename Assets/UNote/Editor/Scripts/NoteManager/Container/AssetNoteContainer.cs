using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class AssetNoteContainer : NoteContainerBase
    {
        #region Define

        [Serializable]
        private class AssetInternalContainer
        {
            public List<AssetNote> m_assetNoteList = new List<AssetNote>();
            public List<AssetLeafNote> m_assetLeafNoteList = new List<AssetLeafNote>();
        }

        #endregion // Define

        #region Field

        public string test;
        [SerializeField] private AssetInternalContainer m_assetNoteContainer;

        private Dictionary<string, AssetInternalContainer> m_assetNoteDict = new();
        private Dictionary<string, List<AssetLeafNote>> m_assetNoteDictByTitle = new();
        
        #endregion // Field
        
        #region Property

        protected override string Identifier => "asset";
        protected override string SubDirectoryName => "Asset";

        #endregion
        
        public override void Load()
        {
            Load<AssetInternalContainer>();
        }

        protected override void LoadData<T>(T container, string authorName)
        {
            if (container is AssetInternalContainer assetContainer)
            {
                m_assetNoteDict[authorName] = assetContainer;
            }
        }

        public override void Save()
        {
            Save(GetContainerSafe());
        }

        private AssetInternalContainer GetContainerSafe(string authorName = null)
        {
            authorName ??= UserConfig.GetUNoteSetting().UserName;
            
            if (!m_assetNoteDict.ContainsKey(authorName))
            {
                m_assetNoteContainer = new AssetInternalContainer();
                m_assetNoteDict.Add(authorName, m_assetNoteContainer);
            }

            return m_assetNoteDict[authorName];
        }

        public List<AssetNote> GetOwnAssetNoteList() => GetContainerSafe().m_assetNoteList;
        public List<AssetLeafNote> GetOwnAssetLeafNoteList() => GetContainerSafe().m_assetLeafNoteList;

        public IEnumerable<List<AssetNote>> GetAssetNoteListAll() =>
            m_assetNoteDict.Values.Where(t => t != null).Select(t => t.m_assetNoteList);

        public IEnumerable<List<AssetLeafNote>> GetAssetLeafNoteListAll() =>
            m_assetNoteDict.Values.Select(t => t.m_assetLeafNoteList);

        public AssetNote GetAssetNoteByGuid(string guid)
        {
            foreach (var noteList in GetAssetNoteListAll())
            {
                foreach (var assetNote in noteList)
                {
                    if (assetNote.NoteId == guid)
                    {
                        return assetNote;
                    }
                }
            }

            return null;
        }

        public List<AssetLeafNote> GetAssetLeafNoteListByGuid(string guid)
        {
            if (m_assetNoteDictByTitle.TryGetValue(guid, out var leafNote))
            {
                return leafNote;
            }

            List<AssetLeafNote> newList = new(64);

            foreach (var noteList in GetAssetLeafNoteListAll())
            {
                foreach (var note in noteList)
                {
                    if (note.NoteId == guid)
                    {
                        newList.Add(note);
                    }
                }
            }

            m_assetNoteDictByTitle[guid] = newList;
            return newList;
        }
        
        public override void ClearCache()
        {
            m_assetNoteDictByTitle.Clear();
        }
    }
}
