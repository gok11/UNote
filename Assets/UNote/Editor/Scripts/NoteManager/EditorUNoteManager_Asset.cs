using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private static AssetNoteContainer s_assetNoteInstance;
        
        private List<AssetNote> m_assetNoteList = new();
        private List<AssetLeafNote> m_assetLeafNoteList = new();

        private Dictionary<string, List<AssetNote>> m_assetNoteDict = new();
        private Dictionary<string, List<AssetLeafNote>> m_assetLeafNoteDict = new();

        #endregion // Field

        private static IReadOnlyList<AssetNote> GetAssetNoteAllList() => Instance.m_assetNoteList;
        private static IReadOnlyList<AssetLeafNote> GetAssetLeafNoteAllList() => Instance.m_assetLeafNoteList;

        #region Initialize

        private static AssetNoteContainer GetOwnAssetNoteContainer()
        {
            if (s_assetNoteInstance)
            {
                return s_assetNoteInstance;
            }

            string dir = Path.Combine(NoteAssetDirectory, "Asset");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_asset.asset");
            AssetNoteContainer container = AssetDatabase.LoadAssetAtPath<AssetNoteContainer>(filePath);
            
            if (container)
            {
                s_assetNoteInstance = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }

            s_assetNoteInstance = ScriptableObject.CreateInstance<AssetNoteContainer>();
            AssetDatabase.CreateAsset(s_assetNoteInstance, filePath);
            return s_assetNoteInstance;
        }
        
        internal static void ReloadAssetNotes()
        {
            ClearAssetNoteCache();
            
            string dir = Path.Combine(NoteAssetDirectory, "Asset");
            foreach (var file in Directory.GetFiles(dir, "*.asset"))
            {
                AssetNoteContainer tmpContainer = AssetDatabase.LoadAssetAtPath<AssetNoteContainer>(file.FullPathToAssetPath());
                Instance.m_assetNoteList.AddRange(tmpContainer.GetAssetNoteList());
                Instance.m_assetLeafNoteList.AddRange(tmpContainer.GetAssetLeafNoteList());
            }
        }

        #endregion

        #region Add Note

        public static AssetNote AddNewAssetNote(string guid, string noteContent)
        {
            AssetNoteContainer container = GetOwnAssetNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Asset Note");
            
            AssetNote newNote = new AssetNote
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                BindAssetId = guid
            };

            container.GetAssetNoteList().Add(newNote);
            container.Save();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }
        
        public static AssetLeafNote AddNewAssetLeafNote(string noteId, string noteContent)
        {
            AssetNoteContainer container = GetOwnAssetNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Asset Leaf Note");
            
            AssetLeafNote newNote = new AssetLeafNote
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                ReferenceNoteId = noteId
            };

            container.GetAssetLeafNoteList().Add(newNote);
            container.Save();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        #endregion

        #region Get Note

        public static List<AssetNote> GetAssetNoteListByGuid(string guid)
        {
            if (Instance.m_assetNoteDict.TryGetValue(guid, out var noteList))
            {
                return noteList;
            }
            
            List<AssetNote> newList = new List<AssetNote>(64);
            
            foreach (var note in Instance.m_assetNoteList)
            {
                if (note.BindAssetId == guid)
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
                AssetNote note = GetAssetNoteListByGuid(key).FirstOrDefault();
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

        #endregion

        #region Delete Note

        private static void DeleteAssetNote(NoteBase note)
        {
            AssetNoteContainer astContainer = GetOwnAssetNoteContainer();
            Undo.RecordObject(astContainer, "Delete Asset Note");
                    
            if (note is AssetNote assetNote)
            {
                List<AssetNote> assetNoteList = astContainer.GetAssetNoteList();
                if (assetNoteList.Contains(assetNote))
                {
                    assetNoteList.Remove(assetNote);
                    astContainer.Save();
                }
            }
            else if (note is AssetLeafNote assetLeafNote)
            {
                List<AssetLeafNote> assetLeafNoteList = astContainer.GetAssetLeafNoteList();
                if (assetLeafNoteList.Contains(assetLeafNote))
                {
                    assetLeafNoteList.Remove(assetLeafNote);
                    astContainer.Save();
                }
            }
        }

        #endregion
        
        #region Clear Cache

        internal static void ClearAssetNoteCache()
        {
            Instance.m_assetNoteList.Clear();
            Instance.m_assetLeafNoteList.Clear();
            Instance.m_assetNoteDict.Clear();
            Instance.m_assetLeafNoteDict.Clear();
        }
        
        #endregion
    }
}
