using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    /// <summary>
    /// Draw note icon on assets have AssetNote in ProjectBrowser
    /// </summary>
    internal static class ProjectBrowserDecorator
    {
        private static Texture2D NoteIcon
        {
            get
            {
                if (s_noteIcon)
                {
                    return s_noteIcon;
                }

                string path = PathUtil.GetTexturePath("note.png");
                s_noteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                return s_noteIcon;
            }
        }

        private static Texture2D s_noteIcon;
        
        [InitializeOnLoadMethod]
        private static void RegisterCallback()
        {
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
        }

        private static void ProjectWindowItemOnGUI(string guid, Rect rect)
        {
            // Get notes link with current asset
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            List<AssetNote> noteList = EditorUNoteManager.GetAssetNoteListByGuid(guid);
            if (noteList == null || noteList.Count == 0)
            {
                return;   
            }
            
            // check current height equal to single line height
            if (rect.height <= EditorGUIUtility.singleLineHeight)
            {
                DrawCountAtRightEdge(noteList, rect);
            }
            else
            {
                DrawCountAtLeftTop(noteList, rect);
            }
        }
        
        private static void DrawCountAtRightEdge(List<AssetNote> noteList, Rect rect)
        {
            // background
            rect.x = rect.xMax - 16;
            rect.width = 14;
            rect.y += 1;
            rect.height -= 2;
            GUI.Box(rect, "");
            
            // note icon
            Rect noteIconRect = rect;
            noteIconRect.x += 4;
            noteIconRect.y += 2;
            noteIconRect.width = noteIconRect.height = 10;
            GUI.DrawTexture(noteIconRect, NoteIcon);
        }

        private static void DrawCountAtLeftTop(List<AssetNote> noteList, Rect rect)
        {
            // background
            rect.x = rect.xMin - 2;
            rect.y += 2;
            rect.width = 14;
            rect.height = 14;
            GUI.Box(rect, "");
            GUI.Box(rect, "");
            GUI.Box(rect, "");
            
            // note icon
            Rect noteIconRect = rect;
            noteIconRect.x += 4;
            noteIconRect.y += 2;
            noteIconRect.width = noteIconRect.height = 10;
            GUI.DrawTexture(noteIconRect, NoteIcon);
        }
    }
}
