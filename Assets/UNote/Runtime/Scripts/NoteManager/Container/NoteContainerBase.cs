using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    public abstract class NoteContainerBase : ScriptableObject
    {
        #region Property
        
        protected abstract string Identifier { get; }

        protected abstract string SubDirectoryName { get; }

        protected virtual string FileDirectory
        {
            get
            {
                string streamingAssets = Application.streamingAssetsPath;
                return Path.Combine(streamingAssets, "UNote", SubDirectoryName);
            }
        }

        protected virtual string OwnFileName
        {
            get { return $"{UserConfig.GetUNoteSetting().UserName}_{Identifier}.json"; }
        }

        protected virtual string OwnFileFullPath
        {
            get { return Path.Combine(FileDirectory, OwnFileName); }
        }
        
        #endregion // Property

        public abstract void Load();
        
        protected void Load<T>() where T : class
        {
            string authorName = UserConfig.GetUNoteSetting().UserName;

            if (File.Exists(OwnFileFullPath))
            {
                string json = File.ReadAllText(OwnFileFullPath);
                var container = JsonUtility.FromJson<T>(json);
                LoadData(container, authorName);
            }
            else
            {
                Save();
            }

            string[] filePaths = Directory.GetFiles(FileDirectory, "*.json");
            foreach (var projectNotePath in filePaths)
            {
                if (projectNotePath == OwnFileFullPath)
                {
                    continue;
                }

                string otherAuthorName = Path.GetFileNameWithoutExtension(projectNotePath);
                string otherJson = File.ReadAllText(projectNotePath);
                var container = JsonUtility.FromJson<T>(otherJson);
                LoadData(container, otherAuthorName);
            }
        }
        
        protected abstract void LoadData<T>(T container, string authorName) where T : class;

        public abstract void Save();
        
        protected void Save<T>(T container) where T : class
        {
            if (!Directory.Exists(FileDirectory))
            {
                Directory.CreateDirectory(FileDirectory);
            }
            
            string json = JsonUtility.ToJson(container);
            File.WriteAllText(OwnFileFullPath, json);

            ClearCache();
        }

        public abstract void ClearCache();
    }
}
