using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    internal class EditorAssetNoteService : EditorNoteServiceBase
    {
        private EditorUNoteManager m_noteManager;
        
        private AssetNoteContainer m_assetNoteInstance;
        
        private List<AssetNote> m_assetNoteList = new();
        private List<AssetNoteMessage> m_assetNoteMessageList = new();

        private List<AssetNote> m_assetNoteListDistinct = new();

        private Dictionary<string, List<AssetNote>> m_assetNoteDict = new();
        private Dictionary<string, List<AssetNoteMessage>> m_assetNoteMessageDict = new();
        
        internal IReadOnlyList<AssetNote> GetAssetNoteAllList() => m_assetNoteList;
        internal IReadOnlyList<AssetNoteMessage> GetAssetNoteMessageAllList() => m_assetNoteMessageList;

        internal EditorAssetNoteService(EditorUNoteManager noteManager)
        {
            m_noteManager = noteManager;
        }
        
        internal AssetNoteContainer GetOwnAssetNoteContainer()
        {
            if (m_assetNoteInstance)
            {
                return m_assetNoteInstance;
            }

            string dir = Path.Combine(NoteAssetDirectory, "Asset");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_asset.asset");
            AssetNoteContainer container = AssetDatabase.LoadAssetAtPath<AssetNoteContainer>(filePath);
            
            if (container)
            {
                m_assetNoteInstance = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }

            m_assetNoteInstance = ScriptableObject.CreateInstance<AssetNoteContainer>();
            AssetDatabase.CreateAsset(m_assetNoteInstance, filePath);
            return m_assetNoteInstance;
        }
        
        internal void ReloadAssetNotes()
        {
            ClearAssetNoteCache();
            
            string dir = Path.Combine(NoteAssetDirectory, "Asset");
            foreach (var file in Directory.GetFiles(dir, "*.asset"))
            {
                AssetNoteContainer tmpContainer = AssetDatabase.LoadAssetAtPath<AssetNoteContainer>(file.FullPathToAssetPath());
                m_assetNoteList.AddRange(tmpContainer.GetAssetNoteList());
                m_assetNoteMessageList.AddRange(tmpContainer.GetAssetNoteMessageList());
            }
        }
        
        public AssetNote AddNewAssetNote(string guid)
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
            
            m_noteManager.TriggerNoteAdded(newNote);
            
            return newNote;
        }
        
        public AssetNoteMessage AddNewAssetNoteMessage(string noteId, string noteContent, List<string> noteTagList)
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
            
            m_noteManager.TriggerNoteAdded(newNote);
            
            return newNote;
        }

        public List<AssetNote> GetAssetNoteListByGuid(string guid)
        {
            if (m_assetNoteDict.TryGetValue(guid, out var noteList))
            {
                return noteList;
            }
            
            List<AssetNote> newList = new List<AssetNote>(64);
            
            foreach (var note in m_assetNoteList)
            {
                if (note.BindAssetId == guid)
                {
                    newList.Add(note);
                }
            }
            m_assetNoteDict.Add(guid, newList);

            return newList;
        }

        public IEnumerable<AssetNote> GetAllAssetNotesIdDistinct()
        {
            if (m_assetNoteListDistinct.Count > 0)
            {
                return m_assetNoteListDistinct;
            }

            foreach (var note in m_assetNoteList)
            {
                bool existBindNote = m_assetNoteListDistinct
                    .FindIndex(t => t.BindAssetId == note.BindAssetId) > 0;
                if (existBindNote)
                {
                    continue;
                }

                m_assetNoteListDistinct.Add(note);
            }

            return m_assetNoteListDistinct;
        }

        public List<AssetNoteMessage> GetAssetNoteMessageListByNoteId(string assetNoteId)
        {
            if (m_assetNoteMessageDict.TryGetValue(assetNoteId, out var noteMessageList))
            {
                return noteMessageList;
            }

            List<AssetNoteMessage> newList = new List<AssetNoteMessage>(64);
            
            // sort by created date
            foreach (var note in m_assetNoteMessageList.OrderBy(t => t.CreatedDate))
            {
                if (note.ReferenceNoteId == assetNoteId)
                {
                    newList.Add(note);
                }
            }
            m_assetNoteMessageDict.Add(assetNoteId, newList);

            return newList;
        }

        internal void DeleteAssetNote(NoteBase note)
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

        internal void ClearAssetNoteCache()
        {
            m_assetNoteList.Clear();
            m_assetNoteMessageList.Clear();
            m_assetNoteListDistinct.Clear();
            m_assetNoteDict.Clear();
            m_assetNoteMessageDict.Clear();
        }
    }
}
