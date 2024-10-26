using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Editor scene note service
    /// </summary>
    internal class EditorSceneNoteService : EditorNoteServiceBase
    {
        private EditorUNoteManager m_noteManager;
        
        private SceneNoteContainer m_sceneNoteContainer;

        private List<SceneNote> m_currentSceneNoteList = new();
        private List<SceneNoteMessage> m_currentSceneMessageList = new();

        private Dictionary<string, List<SceneNote>> m_sceneNoteDict = new();
        private Dictionary<string, List<SceneNoteMessage>> m_sceneMessageDict = new();

        internal IReadOnlyList<SceneNote> GetCurrentSceneNoteList() => m_currentSceneNoteList;
        internal IReadOnlyList<SceneNoteMessage> GetCurrentSceneNoteMessageList() => m_currentSceneMessageList;

        internal EditorSceneNoteService(EditorUNoteManager noteManager)
        {
            m_noteManager = noteManager;
            
            // Reload on scene event
            EditorSceneManager.activeSceneChanged -= OnActiveSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.newSceneCreated -= OnSceneCreated;
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            
            EditorSceneManager.activeSceneChanged += OnActiveSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.newSceneCreated += OnSceneCreated;
            EditorSceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        internal SceneNoteContainer GetOwnSceneNoteContainer()
        {
            if (m_sceneNoteContainer)
            {
                return m_sceneNoteContainer;
            }
            
            string dir = Path.Combine(NoteAssetDirectory, "Scene");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_scene.asset");
            SceneNoteContainer container = AssetDatabase.LoadAssetAtPath<SceneNoteContainer>(filePath);
            
            if (container)
            {
                m_sceneNoteContainer = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }

            m_sceneNoteContainer = ScriptableObject.CreateInstance<SceneNoteContainer>();
            AssetDatabase.CreateAsset(m_sceneNoteContainer, filePath);
            return m_sceneNoteContainer;
        }
        
        /// <summary>
        /// Clear cache and load scene note
        /// </summary>
        internal void ReloadSceneNotes()
        {
            ClearSceneNoteCache();
            
            string dir = Path.Combine(NoteAssetDirectory, "Scene");
            foreach (var file in Directory.GetFiles(dir, "*.asset"))
            {
                SceneNoteContainer tmpContainer = AssetDatabase.LoadAssetAtPath<SceneNoteContainer>(file.FullPathToAssetPath());
                m_currentSceneNoteList.AddRange(tmpContainer.GetCurrentSceneNoteList());
                m_currentSceneMessageList.AddRange(tmpContainer.GetCurrentSceneMessageList());
            }
        }

        /// <summary>
        /// Add new scene note
        /// </summary>
        internal SceneNote AddNewSceneNote()
        {
            SceneNoteContainer container = GetOwnSceneNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Scene Note");

            SceneNote newNote = new SceneNote
            {
                Author = UNoteSetting.UserName,
            };

            string uniqueName = GenerateUniqueName();
            newNote.ChangeNoteName(uniqueName);

            container.GetCurrentSceneNoteList().Add(newNote);
            container.Save();
            
            ReloadSceneNotes();
            
            m_noteManager.TriggerNoteAdded(newNote);

            return newNote;
        }
        
        /// <summary>
        /// Generate unique note name for specified note type
        /// </summary>
        private string GenerateUniqueName()
        {
            const string baseName = "New Note";
            IReadOnlyList<SceneNote> sceneNoteList = GetCurrentSceneNoteList();
            return GetUniqueName(baseName, sceneNoteList);
        }

        /// <summary>
        /// Add new scene note message
        /// </summary>
        internal SceneNoteMessage AddNewSceneNoteMessage(string guid, string noteContent, List<string> noteTagList)
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
            
            m_noteManager.TriggerNoteAdded(newNote);

            return newNote;
        }

        /// <summary>
        /// Get scene note message by scene note GUID
        /// </summary>
        internal List<SceneNoteMessage> GetSceneMessageListByNoteId(string sceneNoteId)
        {
            if (m_sceneMessageDict.TryGetValue(sceneNoteId, out var noteMessageList))
            {
                return noteMessageList;
            }

            List<SceneNoteMessage> newList = new(64);

            foreach (var note in m_currentSceneMessageList.OrderBy(t => t.CreatedDate))
            {
                if (note.ReferenceNoteId == sceneNoteId)
                {
                    newList.Add(note);
                }
            }
            
            m_sceneMessageDict.Add(sceneNoteId, newList);
            return newList;
        }

        /// <summary>
        /// Delete scene note
        /// </summary>
        internal void DeleteSceneNote(NoteBase note)
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
        
        private void OnActiveSceneChanged(Scene prev, Scene current)
        {
            ReloadNotesAndEditorViews();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ReloadNotesAndEditorViews();
        }

        private void OnSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            ReloadNotesAndEditorViews();
        }

        private void OnSceneLoaded(Scene prev, LoadSceneMode current)
        {
            ReloadNotesAndEditorViews();
        }
        
        private void OnSceneClosed(Scene scene)
        {
            ReloadNotesAndEditorViews();
        }

        private void ReloadNotesAndEditorViews()
        {
            ReloadSceneNotes();
            UNoteEditor.CenterPane?.SetupListItems();
            UNoteEditor.RightPane?.SetupMessageList();
        }

        /// <summary>
        /// Clear scene note cache
        /// </summary>
        internal void ClearSceneNoteCache()
        {
            m_currentSceneNoteList.Clear();
            m_currentSceneMessageList.Clear();
            m_sceneNoteDict.Clear();
            m_sceneMessageDict.Clear();
        }
    }
}
