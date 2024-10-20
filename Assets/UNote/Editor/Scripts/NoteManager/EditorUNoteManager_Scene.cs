using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Note manager for SceneNote
    /// </summary>
    public partial class EditorUNoteManager
    {
        private static SceneNoteContainer s_sceneNoteInstance;

        private List<SceneNote> m_currentSceneNoteList = new();
        private List<SceneNoteMessage> m_currentSceneMessageList = new();

        private Dictionary<string, List<SceneNote>> m_sceneNoteDict = new();
        private Dictionary<string, List<SceneNoteMessage>> m_sceneMessageDict = new();

        private static IReadOnlyList<SceneNote> GetCurrentSceneNoteList() => Instance.m_currentSceneNoteList;
        private static IReadOnlyList<SceneNoteMessage> GetCurrentSceneNoteMessageList() => Instance.m_currentSceneMessageList;

        private static SceneNoteContainer GetOwnSceneNoteContainer()
        {
            if (s_sceneNoteInstance)
            {
                return s_sceneNoteInstance;
            }
            
            string dir = Path.Combine(NoteAssetDirectory, "Scene");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_scene.asset");
            SceneNoteContainer container = AssetDatabase.LoadAssetAtPath<SceneNoteContainer>(filePath);
            
            if (container)
            {
                s_sceneNoteInstance = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }

            s_sceneNoteInstance = ScriptableObject.CreateInstance<SceneNoteContainer>();
            AssetDatabase.CreateAsset(s_sceneNoteInstance, filePath);
            return s_sceneNoteInstance;
        }
        
        /// <summary>
        /// Clear cache and load scene note
        /// </summary>
        internal static void ReloadSceneNotes()
        {
            ClearSceneNoteCache();
            
            string dir = Path.Combine(NoteAssetDirectory, "Scene");
            foreach (var file in Directory.GetFiles(dir, "*.asset"))
            {
                SceneNoteContainer tmpContainer = AssetDatabase.LoadAssetAtPath<SceneNoteContainer>(file.FullPathToAssetPath());
                Instance.m_currentSceneNoteList.AddRange(tmpContainer.GetCurrentSceneNoteList());
                Instance.m_currentSceneMessageList.AddRange(tmpContainer.GetCurrentSceneMessageList());
            }
        }

        /// <summary>
        /// Add new scene note
        /// </summary>
        public static SceneNote AddNewSceneNote()
        {
            SceneNoteContainer container = GetOwnSceneNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Scene Note");

            SceneNote newNote = new SceneNote
            {
                Author = UNoteSetting.UserName,
            };

            string uniqueName = GenerateUniqueName(NoteType.Scene);
            newNote.ChangeNoteName(uniqueName);

            container.GetCurrentSceneNoteList().Add(newNote);
            container.Save();
            
            ReloadSceneNotes();
            
            OnNoteAdded?.Invoke(newNote);

            return newNote;
        }

        /// <summary>
        /// Add new scene note message
        /// </summary>
        public static SceneNoteMessage AddNewSceneNoteMessage(string guid, string noteContent, List<string> noteTagList)
        {
            SceneNoteContainer container = GetOwnSceneNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Scene Note");

            SceneNoteMessage newNote = new SceneNoteMessage()
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                ReferenceNoteId = guid,
                NoteTagDataIdList = noteTagList
            };
            
            container.GetCurrentSceneMessageList().Add(newNote);
            container.Save();
            
            ReloadSceneNotes();
            
            OnNoteAdded?.Invoke(newNote);

            return newNote;
        }

        /// <summary>
        /// Get scene note message by scene note GUID
        /// </summary>
        public static List<SceneNoteMessage> GetSceneMessageListByNoteId(string sceneNoteId)
        {
            if (Instance.m_sceneMessageDict.TryGetValue(sceneNoteId, out var noteMessageList))
            {
                return noteMessageList;
            }

            List<SceneNoteMessage> newList = new(64);

            foreach (var note in Instance.m_currentSceneMessageList.OrderBy(t => t.CreatedDate))
            {
                if (note.ReferenceNoteId == sceneNoteId)
                {
                    newList.Add(note);
                }
            }
            
            Instance.m_sceneMessageDict.Add(sceneNoteId, newList);
            return newList;
        }

        /// <summary>
        /// Delete scene note
        /// </summary>
        private static void DeleteSceneNote(NoteBase note)
        {
            SceneNoteContainer container = GetOwnSceneNoteContainer();
            Undo.RecordObject(container, "Delete Scene Note");

            if (note is SceneNote sceneNote)
            {
                List<SceneNote> sceneNoteList = container.GetCurrentSceneNoteList();
                if (sceneNoteList.Contains(sceneNote))
                {
                    sceneNoteList.Remove(sceneNote);
                    container.Save();
                }
            }
            else if (note is SceneNoteMessage sceneNoteMessage)
            {
                List<SceneNoteMessage> sceneMessageList = container.GetCurrentSceneMessageList();
                if (sceneMessageList.Contains(sceneNoteMessage))
                {
                    sceneMessageList.Remove(sceneNoteMessage);
                    container.Save();
                }
            }
            
            ReloadSceneNotes();
        }

        /// <summary>
        /// Clear scene note cache
        /// </summary>
        internal static void ClearSceneNoteCache()
        {
            Instance.m_currentSceneNoteList.Clear();
            Instance.m_currentSceneMessageList.Clear();
            Instance.m_sceneNoteDict.Clear();
            Instance.m_sceneMessageDict.Clear();
        }
    }
}
