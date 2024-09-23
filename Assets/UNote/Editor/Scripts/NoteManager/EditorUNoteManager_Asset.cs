using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Note manager for AssetNote
    /// </summary>
    public partial class EditorUNoteManager
    {
        #region Field

        private static AssetNoteContainer s_assetNoteInstance;
        
        private List<AssetNote> m_assetNoteList = new();
        private List<AssetNoteComment> m_assetNoteCommentList = new();

        private List<AssetNote> m_assetNoteListDistinct = new();

        private Dictionary<string, List<AssetNote>> m_assetNoteDict = new();
        private Dictionary<string, List<AssetNoteComment>> m_assetLeafNoteDict = new();

        #endregion // Field

        private static IReadOnlyList<AssetNote> GetAssetNoteAllList() => Instance.m_assetNoteList;
        private static IReadOnlyList<AssetNoteComment> GetAssetNoteCommentAllList() => Instance.m_assetNoteCommentList;

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
                Instance.m_assetNoteCommentList.AddRange(tmpContainer.GetAssetNoteCommentList());
            }
        }

        #endregion // Initialize

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
            
            ReloadAssetNotes();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }
        
        public static AssetNoteComment AddNewAssetLeafNote(string noteId, string noteContent)
        {
            AssetNoteContainer container = GetOwnAssetNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Asset Leaf Note");
            
            AssetNoteComment newNote = new AssetNoteComment
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                ReferenceNoteId = noteId
            };

            container.GetAssetNoteCommentList().Add(newNote);
            container.Save();
            
            ReloadAssetNotes();
            
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
            if (Instance.m_assetNoteListDistinct.Count > 0)
            {
                return Instance.m_assetNoteListDistinct;
            }

            foreach (var note in Instance.m_assetNoteList)
            {
                bool existBindNote = Instance.m_assetNoteListDistinct
                    .FindIndex(t => t.BindAssetId == note.BindAssetId) > 0;
                if (existBindNote)
                {
                    continue;
                }

                Instance.m_assetNoteListDistinct.Add(note);
            }

            return Instance.m_assetNoteListDistinct;
        }

        public static List<AssetNoteComment> GetAssetNoteCommentListByNoteId(string assetNoteId)
        {
            if (Instance.m_assetLeafNoteDict.TryGetValue(assetNoteId, out var leafNoteList))
            {
                return leafNoteList;
            }

            List<AssetNoteComment> newList = new List<AssetNoteComment>(64);
            
            // sort by created date
            foreach (var note in Instance.m_assetNoteCommentList.OrderBy(t => t.CreatedDate))
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
            else if (note is AssetNoteComment assetLeafNote)
            {
                List<AssetNoteComment> assetLeafNoteList = astContainer.GetAssetNoteCommentList();
                if (assetLeafNoteList.Contains(assetLeafNote))
                {
                    assetLeafNoteList.Remove(assetLeafNote);
                    astContainer.Save();
                }
            }
            
            ReloadAssetNotes();
        }

        #endregion
        
        #region Clear Cache

        internal static void ClearAssetNoteCache()
        {
            Instance.m_assetNoteList.Clear();
            Instance.m_assetNoteCommentList.Clear();
            Instance.m_assetNoteListDistinct.Clear();
            Instance.m_assetNoteDict.Clear();
            Instance.m_assetLeafNoteDict.Clear();
        }
        
        #endregion
    }
}
