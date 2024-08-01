using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class AssetNoteContainer : NoteContainerBase
    {
        #region Field

        [SerializeField] private List<AssetNote> m_assetNoteList;
        [SerializeField] private List<AssetLeafNote> m_assetLeafNoteList;

        private static AssetNoteContainer s_instance;
        
        #endregion // Field
        
        public static AssetNoteContainer GetContainer()
        {
            if (s_instance)
            {
                return s_instance;
            }
            
            string filePath = Path.Combine(FileDirectory, "Asset", $"{UNoteSetting.UserName}_asset.asset");
            AssetNoteContainer container = AssetDatabase.LoadAssetAtPath<AssetNoteContainer>(filePath);

            if (container)
            {
                s_instance = container;
                return container;
            }

            s_instance = CreateInstance<AssetNoteContainer>();
            AssetDatabase.CreateAsset(s_instance, filePath);
            return s_instance;
        }

        public List<AssetNote> GetAssetNoteList() => m_assetNoteList;
        public List<AssetLeafNote> GetAssetLeafNoteList() => m_assetLeafNoteList;
    }
}
