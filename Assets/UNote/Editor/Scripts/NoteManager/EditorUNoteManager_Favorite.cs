using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public partial class EditorUNoteManager
    {
        private static FavoriteNoteContainer s_favoriteNoteContainerInstance;

        private static FavoriteNoteContainer GetOwnFavoriteNoteContainer()
        {
            if (s_favoriteNoteContainerInstance)
            {
                return s_favoriteNoteContainerInstance;
            }
            
            string dir = Path.Combine(NoteAssetDirectory, "Favorite");
            string filePath = Path.Combine(dir, $"{UNoteSetting.UserName}_favorite.asset").FullPathToAssetPath();
            FavoriteNoteContainer container = AssetDatabase.LoadAssetAtPath<FavoriteNoteContainer>(filePath);

            if (container)
            {
                s_favoriteNoteContainerInstance = container;
                return container;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);   
            }
            
            s_favoriteNoteContainerInstance = ScriptableObject.CreateInstance<FavoriteNoteContainer>();
            AssetDatabase.CreateAsset(s_favoriteNoteContainerInstance, filePath);
            AssetDatabase.Refresh();
            return s_favoriteNoteContainerInstance;
        }

        public static IReadOnlyList<string> GetFavoriteNoteList()
        {
            return GetOwnFavoriteNoteContainer().GetFavoriteNoteList();
        }

        public static void AddFavorite(NoteBase note)
        {
            if (note == null)
            {
                return;
            }
            
            List<string> favoriteNoteList = s_favoriteNoteContainerInstance.GetFavoriteNoteList();
            if (favoriteNoteList.Contains(note.NoteId))
            {
                return;
            }
            
            favoriteNoteList.Add(note.NoteId);
            GetOwnFavoriteNoteContainer().Save();
        }

        public static void DeleteFavorite(NoteBase note)
        {
            if (note == null)
            {
                return;
            }
            
            List<string> favoriteNoteList = s_favoriteNoteContainerInstance.GetFavoriteNoteList();
            if (!favoriteNoteList.Contains(note.NoteId))
            {
                return;
            }
            
            favoriteNoteList.Remove(note.NoteId);
            GetOwnFavoriteNoteContainer().Save();
        }
    }
}
