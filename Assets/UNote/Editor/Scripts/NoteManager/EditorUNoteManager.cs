using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using UNote.Runtime;

using Object = UnityEngine.Object;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentNote;

        private ProjectNoteContainer m_projectNoteContainer;
        private AssetNoteContainer m_assetNoteContainer;

        public delegate void AddNoteHandler(NoteBase note);

        public delegate void SelectNoteHandler(NoteBase note);
        public delegate void DeleteNoteHandler(NoteBase note);

        public static event AddNoteHandler OnNoteAdded;
        public static event SelectNoteHandler OnNoteSelected;
        public static event DeleteNoteHandler OnNoteDeleted;

        private static EditorUNoteManager s_instance;

        #endregion // Field

        #region Property

        private static EditorUNoteManager Instance => s_instance ??= new EditorUNoteManager();

        public static NoteType CurrentNoteType => Instance.m_currentNoteType;

        public static NoteBase CurrentNote
        {
            get
            {
                if (Instance.m_currentNote != null)
                {
                    return Instance.m_currentNote;
                }

                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        Instance.m_currentNote = GetProjectNoteAllList()?.FirstOrDefault();
                        break;
                    
                    case NoteType.Asset:
                        Instance.m_currentNote = GetAssetNoteAllList()?.FirstOrDefault();
                        break;
                }

                return Instance.m_currentNote;
            }
        }

        private static string NoteAssetDirectory => Path.Combine("Assets", "UNote", "NoteAssets");

        #endregion // Property

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Initialize own container
            GetOwnProjectNoteContainer();
            GetOwnAssetNoteContainer();
            
            // Load all notes
            ReloadProjectNotes();
            ReloadAssetNotes();
        }

        #region Public Method

        public static void SelectCategory(NoteType noteType)
        {
            Instance.m_currentNote = null;
            Instance.m_currentNoteType = noteType;
            OnNoteSelected?.Invoke(null);
        }

        public static void SelectNote(NoteBase note)
        {
            Instance.m_currentNote = note;
            Instance.m_currentNoteType = note.NoteType;
            OnNoteSelected?.Invoke(note);
        }
        
        public static void ChangeNoteName(NoteBase note, string noteName)
        {
            switch (note?.NoteType)
            {
                case NoteType.Project:
                    if (note is ProjectNote projectNote)
                    {
                        ProjectNoteContainer container =GetOwnProjectNoteContainer();
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

            if (note is RootNoteBase)
            {
                // Reset note selection
                Instance.m_currentNote = null;   
            }
            OnNoteDeleted?.Invoke(note);
        }

        public static void ToggleArchived(NoteBase note)
        {
            note.Archived = !note.Archived;

            SaveAll();
        }

        public static void SaveAll()
        {
            GetOwnProjectNoteContainer().Save();
            GetOwnAssetNoteContainer().Save();
        }
        #endregion // Public Method

        #region Private Method

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

        #endregion // Private Method
    }
}
