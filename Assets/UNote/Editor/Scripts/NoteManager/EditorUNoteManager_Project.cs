using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        #region Field

        private static ProjectNoteContainer s_projectNoteInstance;
        
        private List<ProjectNote> m_projectNoteList = new();
        private List<ProjectLeafNote> m_projectLeafNoteList = new();

        private Dictionary<string, ProjectNote> m_projectNoteDict = new();
        private Dictionary<string, List<ProjectLeafNote>> m_projectNoteDictByGUID = new();
        
        #endregion
        
        internal static IReadOnlyList<ProjectNote> GetProjectNoteAllList() => Instance.m_projectNoteList;
        internal static IReadOnlyList<ProjectLeafNote> GetProjectLeafNoteAllList() => Instance.m_projectLeafNoteList;

        #region Initialize

        private static ProjectNoteContainer GetOwnProjectNoteContainer()
        {
            if (s_projectNoteInstance)
            {
                return s_projectNoteInstance;
            }
            
            string dir = Path.Combine(NoteAssetDirectory, "Project");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_project.asset").FullPathToAssetPath();
            ProjectNoteContainer container = AssetDatabase.LoadAssetAtPath<ProjectNoteContainer>(filePath);

            if (container)
            {
                s_projectNoteInstance = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }
            
            s_projectNoteInstance = ScriptableObject.CreateInstance<ProjectNoteContainer>();
            AssetDatabase.CreateAsset(s_projectNoteInstance, filePath);
            return s_projectNoteInstance;
        }
        
        internal static void ReloadProjectNotes()
        {
            ClearProjectNoteCache();
            
            string dir = Path.Combine(NoteAssetDirectory, "Project");
            foreach (var file in Directory.GetFiles(dir, "*.asset"))
            {
                ProjectNoteContainer tmpContainer = AssetDatabase.LoadAssetAtPath<ProjectNoteContainer>(file.FullPathToAssetPath());
                Instance.m_projectNoteList.AddRange(tmpContainer.GetProjectNoteList());
                Instance.m_projectLeafNoteList.AddRange(tmpContainer.GetProjectLeafNoteList());
            }
        }

        #endregion

        #region Add Note

        public static ProjectNote AddNewProjectNote()
        {
            ProjectNoteContainer container = GetOwnProjectNoteContainer();
            
            Undo.RecordObject(container, "UNote Add New Project Note");
            
            ProjectNote newNote = new ProjectNote
            {
                Author = UNoteSetting.UserName
            };

            string uniqueName = GenerateUniqueName(NoteType.Project);
            newNote.ChangeNoteName(uniqueName);
            
            container.GetProjectNoteList().Add(newNote);
            container.Save();

            ReloadProjectNotes();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }
        
        public static ProjectLeafNote AddNewLeafProjectNote(string guid, string noteContent)
        {
            ProjectNoteContainer container =GetOwnProjectNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Project Note");
            
            ProjectLeafNote newNote = new ProjectLeafNote
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                ReferenceNoteId = guid
            };

            container.GetProjectLeafNoteList().Add(newNote);
            container.Save();
            
            ReloadProjectNotes();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        #endregion

        #region Get Note

        public static List<ProjectLeafNote> GetProjectLeafNoteListByNoteId(string projectNoteId)
        {
            if (Instance.m_projectNoteDictByGUID.TryGetValue(projectNoteId, out var leafNoteList))
            {
                return leafNoteList;
            }

            List<ProjectLeafNote> newList = new(64);

            foreach (var note in Instance.m_projectLeafNoteList)
            {
                if (note.ReferenceNoteId == projectNoteId)
                {
                    newList.Add(note);
                }
            }

            Instance.m_projectNoteDictByGUID.Add(projectNoteId, newList);
            return newList;
        }

        #endregion

        #region Delete Note

        private static void DeleteProjectNote(NoteBase note)
        {
            ProjectNoteContainer projContainer = GetOwnProjectNoteContainer();
            Undo.RecordObject(projContainer, "Delete Project Note");
                    
            if (note is ProjectNote projectNote)
            {
                List<ProjectNote> projectList = projContainer.GetProjectNoteList();
                if (projectList.Contains(projectNote))
                {
                    projectList.Remove(projectNote);
                    projContainer.Save();
                }
            }
            else if (note is ProjectLeafNote projectLeafNote)
            {
                List<ProjectLeafNote> projectList = projContainer.GetProjectLeafNoteList();
                if (projectList.Contains(projectLeafNote))
                {
                    projectList.Remove(projectLeafNote);
                    projContainer.Save();
                }
            }
            
            ReloadProjectNotes();
        }

        #endregion

        #region Clear Cache

        internal static void ClearProjectNoteCache()
        {
            Instance.m_projectNoteList.Clear();
            Instance.m_projectLeafNoteList.Clear();
            Instance.m_projectNoteDict.Clear();
            Instance.m_projectNoteDictByGUID.Clear();
        }

        #endregion
    }
}
