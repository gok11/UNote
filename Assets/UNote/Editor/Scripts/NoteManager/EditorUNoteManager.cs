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
        #region Field

        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentNote;
        
        [SerializeField]
        private NoteQuery m_noteQuery;
        
        private ProjectNoteContainer m_projectNoteContainer;
        private AssetNoteContainer m_assetNoteContainer;

        public delegate void AddNoteHandler(NoteBase note);
        public delegate void SelectNoteHandler(NoteBase note);

        public delegate void SelectQueryHandler(NoteQuery query);

        public delegate void FavoriteNoteHandler(NoteBase note);
        public delegate void ArchiveNoteHandler(NoteBase note);
        public delegate void DeleteNoteHandler(NoteBase note);

        public static event AddNoteHandler OnNoteAdded;
        public static event SelectNoteHandler OnNoteSelected;
        public static event SelectQueryHandler OnNoteQueryUpdated;
        public static event FavoriteNoteHandler OnNoteFavoriteChanged;
        public static event ArchiveNoteHandler OnNoteArchvied;
        public static event DeleteNoteHandler OnNoteDeleted;

        private static EditorUNoteManager s_instance;

        #endregion // Field

        #region Property

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

        #endregion // Property

        #region Initialize

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

        #endregion // Initialize

        #region Public Method

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
            OnNoteQueryUpdated?.Invoke(noteQuery);
        }

        public static void CallUpdateNoteQuery()
        {
            OnNoteQueryUpdated?.Invoke(Instance.m_noteQuery);
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
            List<NoteBase> noteList = new List<NoteBase>();

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

            IEnumerable<NoteBase> notes = noteList;

            // Filter by search text
            string searchText = noteQuery.SearchText;
            if (!searchText.IsNullOrWhiteSpace())
            {
                notes = notes.Where(t => t.NoteName.Contains(searchText) || ContainsSearchTextInComments(t, searchText));
            }

            if (!noteQuery.DisplayArchive)
            {
                notes = notes.Where(t => !t.Archived);
            }
            
            return notes;
        }

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
        
        private static string GetUpdatedDate(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                {
                    ProjectNoteComment comment = EditorUNoteManager
                        .GetProjectNoteCommentListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.UpdatedDate)
                        .FirstOrDefault();
                    return comment != null ? comment.UpdatedDate : note.CreatedDate;
                }

                case NoteType.Asset:
                {
                    AssetNoteComment comment = EditorUNoteManager
                        .GetAssetNoteCommentListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.UpdatedDate)
                        .FirstOrDefault();
                    return comment != null ? comment.UpdatedDate : note.CreatedDate;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static bool ContainsSearchTextInComments(NoteBase note, string text)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                {
                    List<ProjectNoteComment> commentList = EditorUNoteManager
                        .GetProjectNoteCommentListByNoteId(note.NoteId);
                    return commentList.FindIndex(t => t.NoteContent.Contains(text)) >= 0;
                }

                case NoteType.Asset:
                {
                    List<AssetNoteComment> commentList = EditorUNoteManager
                        .GetAssetNoteCommentListByNoteId(note.NoteId);
                    return commentList.FindIndex(t => t.NoteContent.Contains(text)) >= 0;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
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

        public static void ToggleArchived(NoteBase note)
        {
            note.Archived = !note.Archived;
            OnNoteArchvied?.Invoke(note);
            SaveContainerByNoteType(note);
        }

        public static void SaveAll()
        {
            GetOwnProjectNoteContainer().Save();
            GetOwnAssetNoteContainer().Save();
            GetOwnFavoriteNoteContainer().Save();
        }
        #endregion // Public Method

        #region Private Method

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
        
        #endregion // Private Method
    }
}
