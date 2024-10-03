using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    public class CustomQueryContainer : ScriptableObject
    {
        [SerializeField] private List<NoteQuery> m_noteQueryList = new();

        private static CustomQueryContainer s_instance;
        
        internal List<NoteQuery> NoteQueryList => m_noteQueryList;
        
        internal static CustomQueryContainer Get()
        {
            if (s_instance)
            {
                return s_instance;
            }
            
            string dir = Path.Combine("Assets", "UNote", "NoteAssets", "CustomQuery");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_query.asset");
            CustomQueryContainer container = AssetDatabase.LoadAssetAtPath<CustomQueryContainer>(filePath);

            if (container)
            {
                s_instance = container;
                return container;
            }
            
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }

            s_instance = CreateInstance<CustomQueryContainer>();
            AssetDatabase.CreateAsset(s_instance, filePath);
            AssetDatabase.Refresh();
            return s_instance;
        }
        
        internal void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
