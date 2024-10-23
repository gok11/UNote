using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Editor note favorite service
    /// </summary>
    internal class EditorNoteFavoriteService : EditorNoteServiceBase
    {
        private FavoriteNoteContainer m_favoriteNoteContainerInstance;

        internal FavoriteNoteContainer GetOwnFavoriteNoteContainer()
        {
            if (m_favoriteNoteContainerInstance)
            {
                return m_favoriteNoteContainerInstance;
            }
            
            string dir = Path.Combine(NoteAssetDirectory, "Favorite");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_favorite.asset").FullPathToAssetPath();
            FavoriteNoteContainer container = AssetDatabase.LoadAssetAtPath<FavoriteNoteContainer>(filePath);

            if (container)
            {
                m_favoriteNoteContainerInstance = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }
            
            m_favoriteNoteContainerInstance = ScriptableObject.CreateInstance<FavoriteNoteContainer>();
            AssetDatabase.CreateAsset(m_favoriteNoteContainerInstance, filePath);
            AssetDatabase.Refresh();
            return m_favoriteNoteContainerInstance;
        }

        internal IReadOnlyList<string> GetFavoriteNoteList()
        {
            return GetOwnFavoriteNoteContainer().GetFavoriteNoteList();
        }

        internal void AddFavorite(NoteBase note)
        {
            if (note == null)
            {
                return;
            }
            
            List<string> favoriteNoteList = m_favoriteNoteContainerInstance.GetFavoriteNoteList();
            if (favoriteNoteList.Contains(note.NoteId))
            {
                return;
            }
            
            favoriteNoteList.Add(note.NoteId);
            GetOwnFavoriteNoteContainer().Save();
        }

        internal void DeleteFavorite(NoteBase note)
        {
            if (note == null)
            {
                return;
            }
            
            List<string> favoriteNoteList = m_favoriteNoteContainerInstance.GetFavoriteNoteList();
            if (!favoriteNoteList.Contains(note.NoteId))
            {
                return;
            }
            
            favoriteNoteList.Remove(note.NoteId);
            GetOwnFavoriteNoteContainer().Save();
        }
    }
}
