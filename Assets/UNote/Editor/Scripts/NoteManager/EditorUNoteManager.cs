using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Note manager in Unity Editor
    /// </summary>
    [Serializable]
    public class EditorUNoteManager
    {
        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentNote;
        
        [SerializeField]
        private NoteQuery m_noteQuery;
        
        private static EditorProjectNoteService s_projectNoteService;
        private static EditorAssetNoteService s_assetnoteService;
        private static EditorSceneNoteService s_sceneNoteService;
        private static EditorNoteFavoriteService s_noteFavoriteService;
        
        private ProjectNoteContainer m_projectNoteContainer;
        private AssetNoteContainer m_assetNoteContainer;

        // Note callback
        public delegate void AddNoteHandler(NoteBase note);
        public delegate void SelectNoteHandler(NoteBase note);
        public delegate void DeleteNoteHandler(NoteBase note);
        public delegate void FavoriteNoteHandler(NoteBase note);
        public delegate void ArchiveNoteHandler(NoteBase note);
        public static event AddNoteHandler OnNoteAdded;
        public static event SelectNoteHandler OnNoteSelected;
        public static event DeleteNoteHandler OnNoteDeleted;
        public static event FavoriteNoteHandler OnNoteFavoriteChanged;
        public static event ArchiveNoteHandler OnNoteArchived;
        
        // Query callback
        public delegate void AddQueryHandler(NoteQuery query);
        public delegate void SelectQueryHandler(NoteQuery query);

        public delegate void DeleteQueryHandler(NoteQuery query);

        public static event AddQueryHandler OnNoteQueryAdded;
        public static event SelectQueryHandler OnNoteQuerySelected;
        public static event DeleteQueryHandler OnNoteQueryDeleted;


        private static EditorUNoteManager s_instance;
        
        private static EditorUNoteManager Instance => s_instance ??= new EditorUNoteManager();

        public static NoteType CurrentNoteType => Instance.m_currentNoteType;

        public static NoteQuery CurrentNoteQuery => Instance.m_noteQuery ?? (Instance.m_noteQuery = new AllNotesQuery());

        public static NoteBase CurrentNote
        {
            get
            {
                if (Instance.m_currentNote != null)
                {
                    return Instance.m_currentNote;
                }

                if (Instance.m_noteQuery != null)
                {
                    IEnumerable<NoteBase> notes = GetFilteredNotes(Instance.m_noteQuery);
                    notes = SortNotes(notes, Instance.m_noteQuery);
                    
                    SelectNote(notes?.FirstOrDefault());
                    return Instance.m_currentNote;
                }

                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        SelectNote(s_projectNoteService.GetProjectNoteAllList()?.FirstOrDefault());
                        break;
                    
                    case NoteType.Asset:
                        SelectNote(s_assetnoteService.GetAssetNoteAllList()?.FirstOrDefault());
                        break;
                    
                    case NoteType.Scene:
                        SelectNote(s_sceneNoteService.GetCurrentSceneNoteList()?.FirstOrDefault());
                        break;
                    
                    default:
                        throw new NotImplementedException();
                }

                return Instance.m_currentNote;
            }
        }

        private static string NoteAssetDirectory => Path.Combine("Assets", "UNote", "NoteAssets");
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Initialize services
            s_projectNoteService = new EditorProjectNoteService(Instance);
            s_assetnoteService = new EditorAssetNoteService(Instance);
            s_sceneNoteService = new EditorSceneNoteService(Instance);
            s_noteFavoriteService = new EditorNoteFavoriteService();
            
            EditorApplication.delayCall += () =>
            {
                // Initialize own container
                s_projectNoteService.GetOwnProjectNoteContainer();
                s_assetnoteService.GetOwnAssetNoteContainer();
                s_sceneNoteService.GetOwnSceneNoteContainer();

                // Load all notes
                s_projectNoteService.ReloadProjectNotes();
                s_assetnoteService.ReloadAssetNotes();
                s_sceneNoteService.ReloadSceneNotes();
            };
        }

        public static void SelectCategory(NoteType noteType)
        {
            Instance.m_currentNote = null;
            Instance.m_currentNoteType = noteType;
            OnNoteSelected?.Invoke(null);
        }

        public static void SetNoteQuery(NoteQuery noteQuery)
        {
            Instance.m_currentNote = null;
            Instance.m_noteQuery = noteQuery;
            OnNoteQuerySelected?.Invoke(noteQuery);
        }

        public static void CallUpdateNoteQuery()
        {
            OnNoteQuerySelected?.Invoke(Instance.m_noteQuery);
        }

        public static void SelectNote(NoteBase note)
        {
            Instance.m_currentNote = note;
            if (note != null)
            {
                Instance.m_currentNoteType = note.NoteType;   
            }
            OnNoteSelected?.Invoke(note);
        }

        /// <summary>
        /// Get notes filtered by a query
        /// </summary>
        public static IEnumerable<NoteBase> GetFilteredNotes(NoteQuery noteQuery)
        {
            List<RootNoteBase> noteList = new List<RootNoteBase>();

            switch (noteQuery.NoteTypeFilter)
            {
                case NoteTypeFilter.All:
                    noteList.AddRange(s_projectNoteService.GetProjectNoteAllList());
                    noteList.AddRange(s_assetnoteService.GetAllAssetNotesIdDistinct());
                    noteList.AddRange(s_sceneNoteService.GetCurrentSceneNoteList());
                    break;
                
                case NoteTypeFilter.Project:
                    noteList.AddRange(s_projectNoteService.GetProjectNoteAllList());
                    break;
                
                case NoteTypeFilter.Asset:
                    noteList.AddRange(s_assetnoteService.GetAllAssetNotesIdDistinct());
                    break;
                
                case NoteTypeFilter.Scene:
                    noteList.AddRange(s_sceneNoteService.GetCurrentSceneNoteList());
                    break;
            }

            IEnumerable<RootNoteBase> notes = noteList;

            // Filter by archive state
            if (!noteQuery.DisplayArchive)
            {
                notes = notes.Where(t => !t.Archived);
            }
            
            // Filter by note tag
            NoteTags tags = noteQuery.SearchTags;
            if (tags != NoteTags.All)
            {
                // Need filtering
                notes = notes.Where(t => CheckTagInMessages(t, tags));
            }
            
            // Filter by search text
            string searchText = noteQuery.SearchText;
            if (!searchText.IsNullOrWhiteSpace())
            {
                IEnumerable<RootNoteBase> rootNotes = notes as RootNoteBase[] ?? notes.ToArray();
                
                // Return note with matching ID
                RootNoteBase idNote = GetNoteByIdInMessages(rootNotes, searchText);
                if (idNote != null)
                {
                    noteList.Clear();
                    noteList.Add(idNote);
                    return noteList;
                }
                
                searchText = searchText.ToLower();
                notes = rootNotes.Where(t => t.NoteName.ToLower().Contains(searchText) || ContainsSearchTextInMessages(t, searchText));
            }
            
            return notes;
        }

        /// <summary>
        /// Check messages has specified tag
        /// "tags" must be specified single tag
        /// </summary>
        internal static bool CheckTagInMessages(NoteBase note, NoteTags tags)
        {
            NoteTags[] filterTags = NoteTagsUtl.ValidTags
                .Where(t => (t & tags) != 0)
                .ToArray();
            
            switch (note.NoteType)
            {
                case NoteType.Project:
                {
                    List<ProjectNoteMessage> messageList = s_projectNoteService.GetProjectNoteMessageListByNoteId(note.NoteId);
                    return CheckMessageListInternal(messageList);
                }

                case NoteType.Asset:
                {
                    List<AssetNoteMessage> messageList = s_assetnoteService.GetAssetNoteMessageListByNoteId(note.NoteId);
                    return CheckMessageListInternal(messageList);
                }

                case NoteType.Scene:
                {
                    List<SceneNoteMessage> messageList = s_sceneNoteService.GetSceneMessageListByNoteId(note.NoteId);
                    return CheckMessageListInternal(messageList);
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Internal method
            bool CheckMessageListInternal<T>(List<T> messageList) where T : NoteMessageBase
            {
                // None: all message has no tag
                if (tags == NoteTags.None)
                {
                    return messageList.TrueForAll(
                        t => t.NoteTagDataIdList == null || t.NoteTagDataIdList.Count == 0);
                }
                    
                // Tag check
                foreach (var message in messageList)
                {
                    List<string> tagIdList = message.NoteTagDataIdList;
                    if (tagIdList == null || tagIdList.Count == 0)
                    {
                        continue;
                    }

                    foreach (var tag in filterTags)
                    {
                        if (message.NoteTagDataIdList.Contains(tag.ToNoteId()))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Sort notes by NoteQuery
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="noteQuery"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEnumerable<NoteBase> SortNotes(IEnumerable<NoteBase> notes, NoteQuery noteQuery)
        {
            // Sort
            switch (noteQuery.NoteQuerySort)
            {
                case NoteQuerySort.UpdateDate:
                    notes = notes.OrderByDescending(GetUpdatedDate);
                    break;
                case NoteQuerySort.UpdateDateAscending:
                    notes = notes.OrderBy(GetUpdatedDate);
                    break;
                case NoteQuerySort.CreateDate:
                    notes = notes.OrderByDescending(t => t.CreatedDate);
                    break;
                case NoteQuerySort.CreateDateAscending:
                    notes = notes.OrderBy(t => t.CreatedDate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return notes.OrderByDescending(t => t.IsFavorite());
        }
        
        /// <summary>
        /// Get updated date of the message associated with note
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        private static string GetUpdatedDate(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                {
                    ProjectNoteMessage message = s_projectNoteService.GetProjectNoteMessageListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.UpdatedDate)
                        .FirstOrDefault();
                    return message != null ? message.UpdatedDate : note.CreatedDate;
                }

                case NoteType.Asset:
                {
                    AssetNoteMessage message = s_assetnoteService.GetAssetNoteMessageListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.UpdatedDate)
                        .FirstOrDefault();
                    return message != null ? message.UpdatedDate : note.CreatedDate;
                }

                case NoteType.Scene:
                {
                    SceneNoteMessage message = s_sceneNoteService.GetSceneMessageListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.UpdatedDate)
                        .FirstOrDefault();
                    return message != null ? message.UpdatedDate : note.CreatedDate;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get a note with an equal ID 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="idText"></param>
        /// <returns></returns>
        private static RootNoteBase GetNoteByIdInMessages(IEnumerable<RootNoteBase> notes, string idText)
        {
            idText = idText.TrimStart();
            
            if (!idText.StartsWith("nid:"))
            {
                return null;
            }

            idText = idText.Substring("nid:".Length);
            
            return notes.FirstOrDefault(t => t.NoteId == idText);
        }
        
        /// <summary>
        /// Get if the note contains specified text
        /// </summary>
        /// <param name="note"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool ContainsSearchTextInMessages(NoteBase note, string text)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                {
                    List<ProjectNoteMessage> messageList = s_projectNoteService.GetProjectNoteMessageListByNoteId(note.NoteId);
                    return messageList.FindIndex(t => t.NoteContent.ToLower().Contains(text)) >= 0;
                }

                case NoteType.Asset:
                {
                    List<AssetNoteMessage> messageList = s_assetnoteService.GetAssetNoteMessageListByNoteId(note.NoteId);
                    return messageList.FindIndex(t => t.NoteContent.ToLower().Contains(text)) >= 0;
                }

                case NoteType.Scene:
                {
                    List<SceneNoteMessage> messageList = s_sceneNoteService.GetSceneMessageListByNoteId(note.NoteId);
                    return messageList.FindIndex(t => t.NoteContent.ToLower().Contains(text)) >= 0;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Change note name
        /// </summary>
        /// <param name="note"></param>
        /// <param name="noteName"></param>
        public static void ChangeNoteName(NoteBase note, string noteName)
        {
            switch (note?.NoteType)
            {
                case NoteType.Project:
                    if (note is ProjectNote projectNote)
                    {
                        ProjectNoteContainer container = s_projectNoteService.GetOwnProjectNoteContainer();
                        Undo.RecordObject(container, "UNote Change Project Note Title");
                        projectNote.ChangeNoteName(noteName);
                        container.Save();
                    }
                    break;
                
                case NoteType.Scene:
                    if (note is SceneNote sceneNote)
                    {
                        SceneNoteContainer container = s_sceneNoteService.GetOwnSceneNoteContainer();
                        Undo.RecordObject(container, "UNote Change Scene Note Title");
                        sceneNote.ChangeNoteName(noteName);
                        container.Save();
                    }
                    break;
            }
        }

        /// <summary>
        /// Delete note from container
        /// </summary>
        /// <param name="note"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void DeleteNote(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                    s_projectNoteService.DeleteProjectNote(note);
                    break;
                
                case NoteType.Asset:
                    s_assetnoteService.DeleteAssetNote(note);
                    break;

                case NoteType.Scene:
                {
                    s_sceneNoteService.DeleteSceneNote(note);
                    break;
                }
                
                default:
                    throw new NotImplementedException();
            }

            SaveContainerByNoteType(note);

            if (note is RootNoteBase)
            {
                // Reset note selection
                Instance.m_currentNote = null;   
            }
            OnNoteDeleted?.Invoke(note);
        }

        /// <summary>
        /// Toggle favorite state
        /// </summary>
        /// <param name="note"></param>
        public static void ToggleFavorite(NoteBase note)
        {
            bool isFavorite = note.IsFavorite();
            if (isFavorite)
            {
                s_noteFavoriteService.DeleteFavorite(note);
            }
            else
            {
                AddFavorite(note);
            }
            OnNoteFavoriteChanged?.Invoke(note);
            s_noteFavoriteService.GetOwnFavoriteNoteContainer().Save();
        }

        /// <summary>
        /// Toggle archive state
        /// </summary>
        /// <param name="note"></param>
        public static void ToggleArchived(NoteBase note)
        {
            note.Archived = !note.Archived;
            OnNoteArchived?.Invoke(note);
            SaveContainerByNoteType(note);
        }

        /// <summary>
        /// Add query into container
        /// </summary>
        /// <returns></returns>
        public static NoteQuery AddQuery()
        {
            CustomQueryContainer container = CustomQueryContainer.Get();
            
            NoteQuery newQuery = new NoteQuery();
            container.NoteQueryList.Add(newQuery);
            container.Save();

            OnNoteQueryAdded?.Invoke(newQuery);
            return newQuery;
        }

        /// <summary>
        /// Delete query from container
        /// </summary>
        /// <param name="noteQuery"></param>
        public static void DeleteQuery(NoteQuery noteQuery)
        {
            CustomQueryContainer container = CustomQueryContainer.Get();
            int sourceIndex = container.NoteQueryList.FindIndex(t => t.QueryID == noteQuery.QueryID);
            if (sourceIndex >= 0)
            {
                container.NoteQueryList.RemoveAt(sourceIndex);   
            }
            container.Save();
            
            OnNoteQueryDeleted?.Invoke(noteQuery);
        }
        
        /// <summary>
        /// Save all note container 
        /// </summary>
        public static void SaveAll()
        {
            s_projectNoteService.GetOwnProjectNoteContainer().Save();
            s_assetnoteService.GetOwnAssetNoteContainer().Save();
            s_noteFavoriteService.GetOwnFavoriteNoteContainer().Save();
            s_sceneNoteService.GetOwnSceneNoteContainer().Save();
        }

        /// <summary>
        /// Save container by note type
        /// </summary>
        /// <param name="note"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void SaveContainerByNoteType(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                    s_projectNoteService.GetOwnProjectNoteContainer().Save();
                    break;
                
                case NoteType.Asset:
                    s_assetnoteService.GetOwnAssetNoteContainer().Save();
                    break;
                
                case NoteType.Scene:
                    s_sceneNoteService.GetOwnSceneNoteContainer().Save();
                    break;
                
                default:
                    throw new NotImplementedException();
            }
        }

        internal void TriggerNoteAdded(NoteBase note)
        {
            OnNoteAdded?.Invoke(note);
        }
        
        /// <summary>
        /// Clear cache and load note
        /// </summary>
        internal static void ReloadNotes(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.Project:
                    s_projectNoteService.ReloadProjectNotes();
                    break;

                case NoteType.Asset:
                    s_assetnoteService.ReloadAssetNotes();
                    break;
                case NoteType.Scene:
                    s_sceneNoteService.ReloadSceneNotes();
                    break;
            }
            
            s_projectNoteService.ReloadProjectNotes();
        }

        /// <summary>
        /// Get asset note by Object GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static List<AssetNote> GetAssetNoteListByGuid(string guid)
        {
            return s_assetnoteService.GetAssetNoteListByGuid(guid);
        }

        /// <summary>
        /// Get project note message by project note GUID
        /// </summary>
        public static List<ProjectNoteMessage> GetProjectNoteMessageListByNoteId(string projectNoteId)
        {
            return s_projectNoteService.GetProjectNoteMessageListByNoteId(projectNoteId);
        }

        /// <summary>
        /// Get asset note message by asset note GUID
        /// </summary>
        public static List<AssetNoteMessage> GetAssetNoteMessageListByNoteId(string assetNoteId)
        {
            return s_assetnoteService.GetAssetNoteMessageListByNoteId(assetNoteId);
        }
        
        /// <summary>
        /// Get scene note message by scene note GUID
        /// </summary>
        public static List<SceneNoteMessage> GetSceneMessageListByNoteId(string sceneNoteId)
        {
            return s_sceneNoteService.GetSceneMessageListByNoteId(sceneNoteId);
        }

        /// <summary>
        /// Add new project note
        /// </summary>
        /// <returns></returns>
        public static ProjectNote AddNewProjectNote()
        {
            return s_projectNoteService.AddNewProjectNote();
        }
        
        /// <summary>
        /// Add new asset note
        /// </summary>
        public static AssetNote AddNewAssetNote(string guid)
        {
            return s_assetnoteService.AddNewAssetNote(guid);
        }
        
        /// <summary>
        /// Add new scene note
        /// </summary>
        public static SceneNote AddNewSceneNote()
        {
            return s_sceneNoteService.AddNewSceneNote();
        }

        /// <summary>
        /// Add new scene note message
        /// </summary>
        public static ProjectNoteMessage AddNewProjectNoteMessage(string guid, string noteContent, List<string> noteTagList)
        {
            return s_projectNoteService.AddNewProjectNoteMessage(guid, noteContent, noteTagList);
        }
        
        /// <summary>
        /// Add new asset note message
        /// </summary>
        public static AssetNoteMessage AddNewAssetNoteMessage(string guid, string noteContent, List<string> noteTagList)
        {
            return s_assetnoteService.AddNewAssetNoteMessage(guid, noteContent, noteTagList);
        }
        
        /// <summary>
        /// Add new scene note message
        /// </summary>
        public static SceneNoteMessage AddNewSceneNoteMessage(string guid, string noteContent, List<string> noteTagList)
        {
            return s_sceneNoteService.AddNewSceneNoteMessage(guid, noteContent, noteTagList);
        }

        /// <summary>
        /// Mark specified note as favorite
        /// </summary>
        /// <param name="note"></param>
        public static void AddFavorite(NoteBase note)
        {
            s_noteFavoriteService.AddFavorite(note);
        }

        /// <summary>
        /// Get notes marked as favorite
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<string> GetFavoriteNoteList()
        {
            return s_noteFavoriteService.GetFavoriteNoteList();
        }
    }
}
