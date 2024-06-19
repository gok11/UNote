using System;
using System.Collections;
using System.Collections.Generic;
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
            string authorName = UserConfig.GetUNoteSetting().UserName;
            Save(GetContainerSafe(authorName));
        }

        private AssetInternalContainer GetContainerSafe(string authorName)
        {
            if (!m_assetNoteDict.ContainsKey(authorName))
            {
                m_assetNoteContainer = new AssetInternalContainer();
                m_assetNoteDict.Add(authorName, m_assetNoteContainer);
            }

            return m_assetNoteDict[authorName];
        }

        public override void ClearCache()
        {
            m_assetNoteDictByTitle.Clear();
        }
    }
}
