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
            public List<AssetNote> m_assetLeafNoteList = new List<AssetNote>();
        }

        #endregion // Define

        #region Field

        [SerializeField] private AssetInternalContainer m_assetNoteContainer;

        private Dictionary<string, AssetInternalContainer> m_assetNoteDict = new();
        private Dictionary<string, List<AssetNote>> m_assetNoteDictByTitle = new();
        
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

        private AssetInternalContainer GetContainerSafe()
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;
            
            if (!m_assetNoteDict.ContainsKey(authorName))
            {
                m_assetNoteContainer = new AssetInternalContainer();
                m_assetNoteDict.Add(authorName, m_assetNoteContainer);
            }

            return m_assetNoteDict[authorName];
        }

        public List<AssetNote> GetOwnAssetLeafNoteList() => GetContainerSafe().m_assetLeafNoteList;
        
        public IEnumerable<List<AssetNote>> GetAssetLeafNoteListAll() =>
            m_assetNoteDict.Values.Select(t => t.m_assetLeafNoteList);

        public List<AssetNote> GetAssetLeafNoteListByGuid(string guid)
        {
            if (m_assetNoteDictByTitle.TryGetValue(guid, out var leafNote))
            {
                return leafNote;
            }

            List<AssetNote> newList = new(64);

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
