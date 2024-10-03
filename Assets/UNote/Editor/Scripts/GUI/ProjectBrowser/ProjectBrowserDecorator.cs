using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UNote.Editor
{
    public class ProjectBrowserDecorator
    {
        private const string NoteIconGUID = "1c6c4635976f8694bb0178d15ed89627";
        
        private static Texture2D NoteIcon
        {
            get
            {
                if (s_noteIcon)
                {
                    return s_noteIcon;
                }

                string path = AssetDatabase.GUIDToAssetPath(NoteIconGUID);
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

            IReadOnlyList<AssetNoteMessage> noteList = EditorUNoteManager.GetAssetNoteMessageListByNoteId(guid);
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
        
        private static void DrawCountAtRightEdge(IReadOnlyList<AssetNoteMessage> noteList, Rect rect)
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

        private static void DrawCountAtLeftTop(IReadOnlyList<AssetNoteMessage> noteList, Rect rect)
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
