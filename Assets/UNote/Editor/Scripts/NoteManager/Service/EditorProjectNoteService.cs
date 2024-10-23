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
    /// Editor project note service
    /// </summary>
    internal class EditorProjectNoteService : EditorNoteServiceBase
    {
        private EditorUNoteManager m_noteManager;
        
        private ProjectNoteContainer m_projectNoteContainer;
        
        private List<ProjectNote> m_projectNoteList = new();
        private List<ProjectNoteMessage> m_projectNoteMessageList = new();

        private Dictionary<string, ProjectNote> m_projectNoteDict = new();
        private Dictionary<string, List<ProjectNoteMessage>> m_projectMessageDictByGUID = new();
        
        internal IReadOnlyList<ProjectNote> GetProjectNoteAllList() => m_projectNoteList;
        internal IReadOnlyList<ProjectNoteMessage> GetProjectNoteMessageAllList() => m_projectNoteMessageList;

        internal EditorProjectNoteService(EditorUNoteManager noteManager)
        {
            m_noteManager = noteManager;
        }
        
        internal ProjectNoteContainer GetOwnProjectNoteContainer()
        {
            if (m_projectNoteContainer)
            {
                return m_projectNoteContainer;
            }
            
            string dir = Path.Combine(NoteAssetDirectory, "Project");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_project.asset").FullPathToAssetPath();
            ProjectNoteContainer container = AssetDatabase.LoadAssetAtPath<ProjectNoteContainer>(filePath);

            if (container)
            {
                m_projectNoteContainer = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }
            
            m_projectNoteContainer = ScriptableObject.CreateInstance<ProjectNoteContainer>();
            AssetDatabase.CreateAsset(m_projectNoteContainer, filePath);
            AssetDatabase.Refresh();
            return m_projectNoteContainer;
        }
        
        internal void ReloadProjectNotes()
        {
            ClearProjectNoteCache();
            
            string dir = Path.Combine(NoteAssetDirectory, "Project");
            foreach (var file in Directory.GetFiles(dir, "*.asset"))
            {
                ProjectNoteContainer tmpContainer = AssetDatabase.LoadAssetAtPath<ProjectNoteContainer>(file.FullPathToAssetPath());
                m_projectNoteList.AddRange(tmpContainer.GetProjectNoteList());
                m_projectNoteMessageList.AddRange(tmpContainer.GetProjectMessageList());
            }
        }

        internal ProjectNote AddNewProjectNote()
        {
            ProjectNoteContainer container = GetOwnProjectNoteContainer();
            
            Undo.RecordObject(container, "UNote Add New Project Note");
            
            ProjectNote newNote = new ProjectNote
            {
                Author = UNoteSetting.UserName
            };

            string uniqueName = GenerateUniqueName();
            newNote.ChangeNoteName(uniqueName);
            
            container.GetProjectNoteList().Add(newNote);
            container.Save();

            ReloadProjectNotes();
            
            m_noteManager.TriggerNoteAdded(newNote);
            
            return newNote;
        }

        private string GenerateUniqueName()
        {
            const string baseName = "New Note";
            var projectNoteList = GetProjectNoteAllList();
            return GetUniqueName(baseName, projectNoteList);
        }
        
        internal ProjectNoteMessage AddNewProjectNoteMessage(string guid, string noteContent, List<string> noteTagList)
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
            
            m_noteManager.TriggerNoteAdded(newNote);
            
            return newNote;
        }

        internal List<ProjectNoteMessage> GetProjectNoteMessageListByNoteId(string projectNoteId)
        {
            if (m_projectMessageDictByGUID.TryGetValue(projectNoteId, out var noteMessageList))
            {
                return noteMessageList;
            }

            List<ProjectNoteMessage> newList = new(64);

            // sort by created date
            foreach (var note in m_projectNoteMessageList.OrderBy(t => t.CreatedDate))
            {
                if (note.ReferenceNoteId == projectNoteId)
                {
                    newList.Add(note);
                }
            }

            m_projectMessageDictByGUID.Add(projectNoteId, newList);
            return newList;
        }

        internal void DeleteProjectNote(NoteBase note)
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

        internal void ClearProjectNoteCache()
        {
            m_projectNoteList.Clear();
            m_projectNoteMessageList.Clear();
            m_projectNoteDict.Clear();
            m_projectMessageDictByGUID.Clear();
        }
    }
}
