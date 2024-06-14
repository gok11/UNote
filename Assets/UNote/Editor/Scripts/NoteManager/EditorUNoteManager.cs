using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class EditorUNoteManager
    {
        #region Field

        private NoteType m_currentNoteType = NoteType.Project;
        private NoteBase m_currentRootNote;
        private NoteBase m_currentLeafNote;

        private ProjectNoteContainer m_projectNoteContainer;
        private ProjectNoteIdConvertData m_projectNoteIdConvertData;

        private static EditorUNoteManager s_instance;

        #endregion // Field

        #region Property

        public static EditorUNoteManager Instance => s_instance ??= new EditorUNoteManager();

        public static NoteType CurrentNoteType => Instance.m_currentNoteType;

        public static NoteBase CurrentRootNote
        {
            get
            {
                if (Instance.m_currentRootNote != null)
                {
                    return Instance.m_currentRootNote;
                }

                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        Instance.m_currentRootNote = GetAllProjectNotes()?.FirstOrDefault();
                        break;
                }

                return Instance.m_currentRootNote;
            }
        }

        public static NoteBase CurrentLeafNote
        {
            get
            {
                if (Instance.m_currentLeafNote != null)
                {
                    return Instance.m_currentLeafNote;
                }

                switch (Instance.m_currentNoteType)
                {
                    case NoteType.Project:
                        string noteId = CurrentRootNote.NoteId;
                        Instance.m_currentLeafNote = GetAllProjectLeafNotes()
                            .FirstOrDefault(t => t.NoteId == noteId);
                        break;
                }

                return Instance.m_currentLeafNote;
            }
        }

        #endregion // Property

        #region Constructor
        static EditorUNoteManager()
        {
            InitializeEditorNote();
        }
        #endregion // Constructor

        #region Public Method
        [InitializeOnLoadMethod]
        public static void InitializeEditorNote()
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;

            Instance.m_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
            Instance.m_projectNoteContainer.Load();

            Instance.m_projectNoteIdConvertData =
                ScriptableObject.CreateInstance<ProjectNoteIdConvertData>();
            Instance.m_projectNoteIdConvertData.Load(authorName);
        }

        public static void SelectCategory(NoteType noteType)
        {
            Instance.m_currentRootNote = null;
            Instance.m_currentLeafNote = null;

            Instance.m_currentNoteType = noteType;
        }

        public static void SelectRoot(NoteBase note)
        {
            Instance.m_currentRootNote = note;
            Instance.m_currentLeafNote = null;

            Instance.m_currentNoteType = note.NoteType;
        }

        public static void SelectLeaf(NoteBase note)
        {
            Instance.m_currentLeafNote = note;
            Instance.m_currentNoteType = note.NoteType;

            // Root を探して設定
            switch (note.NoteType)
            {
                case NoteType.Project:
                    string projectNoteId = note.NoteId;
                    var projectNotes = GetAllProjectNotes();
                    foreach (var projectNote in projectNotes)
                    {
                        if (projectNote.NoteId == projectNoteId)
                        {
                            Instance.m_currentRootNote = projectNote;
                            break;
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        #region Project Note

        public static SerializedObject GetProjectNoteContainerObject()
        {
            return new SerializedObject(Instance.m_projectNoteContainer);
        }

        public static SerializedObject ProjectNoteIdConvertDataObject()
        {
            return new SerializedObject(Instance.m_projectNoteIdConvertData);
        }

        public static ProjectNote AddNewProjectNote()
        {
            ProjectNote newNote = new ProjectNote();
            newNote.Author = UserConfig.GetUNoteSetting().UserName;

            string uniqueName = GenerateUniqueName(NoteType.Project);
            Instance.m_projectNoteIdConvertData.SetTitle(newNote.NoteId, uniqueName);
            ProjectNoteIDManager.ResetData();

            Undo.RecordObject(Instance.m_projectNoteContainer, "UNote Add New Project Note");

            Instance.m_projectNoteContainer.GetOwnProjectNoteList().Add(newNote);
            Instance.m_projectNoteContainer.Save();
            
            return newNote;
        }

        public static ProjectLeafNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            ProjectLeafNote newNote = new ProjectLeafNote();
            newNote.Author = UserConfig.GetUNoteSetting().UserName;
            newNote.NoteContent = noteContent;
            newNote.NoteId = guid;

            Undo.RecordObject(Instance.m_projectNoteContainer, "UNote Add New Project Note");
            
            Instance.m_projectNoteContainer.GetOwnProjectLeafNoteList().Add(newNote);
            Instance.m_projectNoteContainer.Save();
            
            return newNote;
        }

        public static IEnumerable<ProjectNote> GetAllProjectNotes()
        {
            return Instance.m_projectNoteContainer.GetProjectNoteListAll().SelectMany(t => t);
        }

        public static IEnumerable<ProjectLeafNote> GetAllProjectLeafNotes()
        {
            return Instance.m_projectNoteContainer.GetProjectLeafNoteListAll().SelectMany(t => t);
        }

        public static SerializedObject CreateProjectNoteContainerObject()
        {
            return new SerializedObject(Instance.m_projectNoteContainer);
        }

        public static void ChangeProjectNoteTitle(string guid, string newTitle)
        {
            Instance.m_projectNoteIdConvertData.SetTitle(guid, newTitle);
            ProjectNoteIDManager.ResetData();
        }

        #endregion // Project Note

        public static void DeleteNote(NoteBase note)
        {
            switch (note.NoteType)
            {
                case NoteType.Project:
                    if (note is ProjectNote projectNote)
                    {
                        List<ProjectNote> projectList =
                            Instance.m_projectNoteContainer.GetOwnProjectNoteList();
                        if (projectList.Contains(projectNote))
                        {
                            projectList.Remove(projectNote);
                            Instance.m_projectNoteContainer.Save();

                            // 変換情報も削除
                            Instance.m_projectNoteIdConvertData.DeleteTable(projectNote.NoteId);

                            // TODO 他の所属Leaf削除
                        }
                    }
                    else if (note is ProjectLeafNote projectLeafNote)
                    {
                        List<ProjectLeafNote> projectList =
                            Instance.m_projectNoteContainer.GetOwnProjectLeafNoteList();
                        if (projectList.Contains(projectLeafNote))
                        {
                            projectList.Remove(projectLeafNote);
                            Instance.m_projectNoteContainer.Save();

                            // TODO 最後の一つなら変換テーブル削除
                        }
                    }

                    break;
            }
        }

        public static void ToggleArchived(NoteBase note)
        {
            note.Archived = !note.Archived;

            SaveAll();
        }

        public static void SaveAll()
        {
            Instance.m_projectNoteContainer.Save();
            Instance.m_projectNoteIdConvertData.Save();
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
                    var notes = GetAllProjectNotes();
                    while (true)
                    {
                        bool isOverlap = false;
                        foreach (var note in notes)
                        {
                            if (ProjectNoteIDManager.ConvertGuid(note.NoteId) == noteName)
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
