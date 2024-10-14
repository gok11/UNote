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
    public partial class EditorUNoteManager
    {
        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentNote;
        
        [SerializeField]
        private NoteQuery m_noteQuery;
        
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
                        SelectNote(GetProjectNoteAllList()?.FirstOrDefault());
                        break;
                    
                    case NoteType.Asset:
                        SelectNote(GetAssetNoteAllList()?.FirstOrDefault());
                        break;
                }

                return Instance.m_currentNote;
            }
        }

        private static string NoteAssetDirectory => Path.Combine("Assets", "UNote", "NoteAssets");
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += () =>
            {
                // Initialize own container
                GetOwnProjectNoteContainer();
                GetOwnAssetNoteContainer();

                // Load all notes
                ReloadProjectNotes();
                ReloadAssetNotes();
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
                    noteList.AddRange(GetProjectNoteAllList());
                    noteList.AddRange(GetAllAssetNotesIdDistinct());
                    break;
                
                case NoteTypeFilter.Project:
                    noteList.AddRange(GetProjectNoteAllList());
                    break;
                
                case NoteTypeFilter.Asset:
                    noteList.AddRange(GetAllAssetNotesIdDistinct());
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
                
                notes = rootNotes.Where(t => t.NoteName.Contains(searchText) || ContainsSearchTextInMessages(t, searchText));
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
                    List<ProjectNoteMessage> messageList = GetProjectNoteMessageListByNoteId(note.NoteId);
                    return CheckMessageListInternal(messageList);
                }

                case NoteType.Asset:
                {
                    List<AssetNoteMessage> messageList = GetAssetNoteMessageListByNoteId(note.NoteId);
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
                    ProjectNoteMessage message = GetProjectNoteMessageListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.UpdatedDate)
                        .FirstOrDefault();
                    return message != null ? message.UpdatedDate : note.CreatedDate;
                }

                case NoteType.Asset:
                {
                    AssetNoteMessage message = GetAssetNoteMessageListByNoteId(note.NoteId)
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
                    List<ProjectNoteMessage> messageList = GetProjectNoteMessageListByNoteId(note.NoteId);
                    return messageList.FindIndex(t => t.NoteContent.Contains(text)) >= 0;
                }

                case NoteType.Asset:
                {
                    List<AssetNoteMessage> messageList = GetAssetNoteMessageListByNoteId(note.NoteId);
                    return messageList.FindIndex(t => t.NoteContent.Contains(text)) >= 0;
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
                        ProjectNoteContainer container = GetOwnProjectNoteContainer();
                        Undo.RecordObject(container, "UNote Change Project Note Title");
                        projectNote.ChangeNoteName(noteName);
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
                    DeleteProjectNote(note);
                    break;
                
                case NoteType.Asset:
                    DeleteAssetNote(note);
                    break;
                
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
                DeleteFavorite(note);
            }
            else
            {
                AddFavorite(note);
            }
            OnNoteFavoriteChanged?.Invoke(note);
            GetOwnFavoriteNoteContainer().Save();
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
            GetOwnProjectNoteContainer().Save();
            GetOwnAssetNoteContainer().Save();
            GetOwnFavoriteNoteContainer().Save();
        }
        
        /// <summary>
        /// Generate unique note name for specified note type
        /// </summary>
        private static string GenerateUniqueName(NoteType noteType)
        {
            const string baseName = "New Note";

            switch (noteType)
            {
                case NoteType.Project:
                    string noteName = baseName;
                    int id = 0;
                    var notes = GetProjectNoteAllList();
                    while (true)
                    {
                        bool isOverlap = false;
                        foreach (var note in notes)
                        {
                            if (note.NoteName == noteName)
                            {
                                isOverlap = true;
                                break;
                            }
                        }

                        if (!isOverlap)
                        {
                            break;
                        }

                        noteName = $"{baseName} {id++}";
                    }
                    return noteName;

                default:
                    throw new NotImplementedException();
            }
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
                    GetOwnProjectNoteContainer().Save();
                    break;
                
                case NoteType.Asset:
                    GetOwnAssetNoteContainer().Save();
                    break;
                
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
