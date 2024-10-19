using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UNote.Runtime
{
    [Serializable]
    public class SceneNoteInfo
    {
        public string m_sceneGuid;
        public List<SceneNote> m_sceneNoteList = new();
        public List<SceneNoteMessage> m_sceneNoteMessageList = new();

    }
    
    public class SceneNoteContainer : NoteContainerBase
    {
        [SerializeField] private List<SceneNoteInfo> m_sceneNoteInfoList = new();

        private SceneNoteInfo ActiveSceneInfo
        {
            get
            {
                string activeScenePath = EditorSceneManager.GetActiveScene().path;
                string activeSceneGuid = AssetDatabase.AssetPathToGUID(activeScenePath);

                SceneNoteInfo noteInfo = m_sceneNoteInfoList.Find(t => t.m_sceneGuid == activeSceneGuid);
                if (noteInfo == null)
                {
                    noteInfo = new SceneNoteInfo()
                    {
                        m_sceneGuid = activeSceneGuid
                    };
                    m_sceneNoteInfoList.Add(noteInfo);
                }
                
                return noteInfo;
            }   
        }
        
        public List<SceneNote> GetCurrentSceneNoteList()
        {
            return ActiveSceneInfo?.m_sceneNoteList;
        }

        public List<SceneNote> GetSceneNoteListByScenePath(string scenePath)
        {
            string sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
            return GetSceneNoteListBySceneGuid(sceneGuid);
        }

        public List<SceneNote> GetSceneNoteListBySceneGuid(string sceneGuid)
        {
            SceneNoteInfo noteInfo = m_sceneNoteInfoList.Find(t => t.m_sceneGuid == sceneGuid);
            if (noteInfo == null)
            {
                noteInfo = new SceneNoteInfo()
                {
                    m_sceneGuid = sceneGuid
                };
                m_sceneNoteInfoList.Add(noteInfo);
            }

            return noteInfo.m_sceneNoteList;
        }

        public List<SceneNoteMessage> GetCurrentSceneMessageList()
        {
            return ActiveSceneInfo?.m_sceneNoteMessageList;
        }
        
        public List<SceneNoteMessage> GetSceneMessageListByScenePath(string scenePath)
        {
            string sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
            return GetSceneMessageList(sceneGuid);
        }
        
        public List<SceneNoteMessage> GetSceneMessageList(string sceneGuid)
        {
            return m_sceneNoteInfoList.Find(t => t.m_sceneGuid == sceneGuid).m_sceneNoteMessageList;
        }
    }
}
