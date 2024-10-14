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
        private static AssetNoteContainer s_assetNoteInstance;
        
        private List<AssetNote> m_assetNoteList = new();
        private List<AssetNoteMessage> m_assetNoteMessageList = new();

        private List<AssetNote> m_assetNoteListDistinct = new();

        private Dictionary<string, List<AssetNote>> m_assetNoteDict = new();
        private Dictionary<string, List<AssetNoteMessage>> m_assetNoteMessageDict = new();
        
        private static IReadOnlyList<AssetNote> GetAssetNoteAllList() => Instance.m_assetNoteList;
        private static IReadOnlyList<AssetNoteMessage> GetAssetNoteMessageAllList() => Instance.m_assetNoteMessageList;

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
                Instance.m_assetNoteMessageList.AddRange(tmpContainer.GetAssetNoteMessageList());
            }
        }
        
        public static AssetNote AddNewAssetNote(string guid, string noteContent)
        {
            AssetNoteContainer container = GetOwnAssetNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Asset Note");
            
            AssetNote newNote = new AssetNote
            {
                Author = UNoteSetting.UserName,
                BindAssetId = guid
            };

            container.GetAssetNoteList().Add(newNote);
            container.Save();
            
            ReloadAssetNotes();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }
        
        public static AssetNoteMessage AddNewAssetNoteMessage(string noteId, string noteContent, List<string> noteTagList)
        {
            AssetNoteContainer container = GetOwnAssetNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Asset Note Message");
            
            AssetNoteMessage newNote = new AssetNoteMessage
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                ReferenceNoteId = noteId,
                NoteTagDataIdList = noteTagList
            };

            container.GetAssetNoteMessageList().Add(newNote);
            container.Save();
            
            ReloadAssetNotes();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

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

        public static List<AssetNoteMessage> GetAssetNoteMessageListByNoteId(string assetNoteId)
        {
            if (Instance.m_assetNoteMessageDict.TryGetValue(assetNoteId, out var noteMessageList))
            {
                return noteMessageList;
            }

            List<AssetNoteMessage> newList = new List<AssetNoteMessage>(64);
            
            // sort by created date
            foreach (var note in Instance.m_assetNoteMessageList.OrderBy(t => t.CreatedDate))
            {
                if (note.ReferenceNoteId == assetNoteId)
                {
                    newList.Add(note);
                }
            }
            Instance.m_assetNoteMessageDict.Add(assetNoteId, newList);

            return newList;
        }

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
            else if (note is AssetNoteMessage assetNoteMessage)
            {
                List<AssetNoteMessage> assetNoteMessageList = astContainer.GetAssetNoteMessageList();
                if (assetNoteMessageList.Contains(assetNoteMessage))
                {
                    assetNoteMessageList.Remove(assetNoteMessage);
                    astContainer.Save();
                }
            }
            
            ReloadAssetNotes();
        }

        internal static void ClearAssetNoteCache()
        {
            Instance.m_assetNoteList.Clear();
            Instance.m_assetNoteMessageList.Clear();
            Instance.m_assetNoteListDistinct.Clear();
            Instance.m_assetNoteDict.Clear();
            Instance.m_assetNoteMessageDict.Clear();
        }
    }
}
