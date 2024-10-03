using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Note manager for ProjectNote
    /// </summary>
    public partial class EditorUNoteManager
    {
        private static ProjectNoteContainer s_projectNoteInstance;
        
        private List<ProjectNote> m_projectNoteList = new();
        private List<ProjectNoteMessage> m_projectNoteMessageList = new();

        private Dictionary<string, ProjectNote> m_projectNoteDict = new();
        private Dictionary<string, List<ProjectNoteMessage>> m_projectNoteDictByGUID = new();
        
        internal static IReadOnlyList<ProjectNote> GetProjectNoteAllList() => Instance.m_projectNoteList;
        internal static IReadOnlyList<ProjectNoteMessage> GetProjectNoteMessageAllList() => Instance.m_projectNoteMessageList;

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
            AssetDatabase.Refresh();
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
                Instance.m_projectNoteMessageList.AddRange(tmpContainer.GetProjectMessageList());
            }
        }

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
        
        public static ProjectNoteMessage AddNewProjectNoteMessage(string guid, string noteContent, List<string> noteTagList)
        {
            ProjectNoteContainer container = GetOwnProjectNoteContainer();
            
            Undo.RegisterCompleteObjectUndo(container, "UNote Add New Project Note");
            
            ProjectNoteMessage newNote = new ProjectNoteMessage
            {
                Author = UNoteSetting.UserName,
                NoteContent = noteContent,
                ReferenceNoteId = guid,
                NoteTagDataIdList = noteTagList
            };

            container.GetProjectMessageList().Add(newNote);
            container.Save();
            
            ReloadProjectNotes();
            
            OnNoteAdded?.Invoke(newNote);
            
            return newNote;
        }

        public static List<ProjectNoteMessage> GetProjectNoteMessageListByNoteId(string projectNoteId)
        {
            if (Instance.m_projectNoteDictByGUID.TryGetValue(projectNoteId, out var noteMessageList))
            {
                return noteMessageList;
            }

            List<ProjectNoteMessage> newList = new(64);

            // sort by created date
            foreach (var note in Instance.m_projectNoteMessageList.OrderBy(t => t.CreatedDate))
            {
                if (note.ReferenceNoteId == projectNoteId)
                {
                    newList.Add(note);
                }
            }

            Instance.m_projectNoteDictByGUID.Add(projectNoteId, newList);
            return newList;
        }

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
            else if (note is ProjectNoteMessage projectNoteMessage)
            {
                List<ProjectNoteMessage> projectList = projContainer.GetProjectMessageList();
                if (projectList.Contains(projectNoteMessage))
                {
                    projectList.Remove(projectNoteMessage);
                    projContainer.Save();
                }
            }
            
            ReloadProjectNotes();
        }

        internal static void ClearProjectNoteCache()
        {
            Instance.m_projectNoteList.Clear();
            Instance.m_projectNoteMessageList.Clear();
            Instance.m_projectNoteDict.Clear();
            Instance.m_projectNoteDictByGUID.Clear();
        }
    }
}
