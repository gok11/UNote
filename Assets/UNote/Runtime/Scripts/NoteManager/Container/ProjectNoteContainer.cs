using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UNote.Runtime
{
    public class ProjectNoteContainer : NoteContainerBase
    {
        #region Field
        
        [SerializeField] private List<ProjectNote> m_projectNoteList = new List<ProjectNote>();
        [SerializeField] private List<ProjectLeafNote> m_projectLeafNoteList = new List<ProjectLeafNote>();

        private static ProjectNoteContainer s_instance;
        
        #endregion // Field
        
        public static ProjectNoteContainer GetContainer()
        {
            if (s_instance)
            {
                return s_instance;
            }
            
            string filePath = Path.Combine(FileDirectory, "Project", $"{UNoteSetting.UserName}_project.asset");
            ProjectNoteContainer container = AssetDatabase.LoadAssetAtPath<ProjectNoteContainer>(filePath);

            if (container)
            {
                s_instance = container;
                return container;
            }

            s_instance = CreateInstance<ProjectNoteContainer>();
            AssetDatabase.CreateAsset(s_instance, filePath);
            return s_instance;
        }
        
        
        public List<ProjectNote> GetProjectNoteList() => m_projectNoteList;

        public List<ProjectLeafNote> GetProjectLeafNoteList() => m_projectLeafNoteList;
    }
}
