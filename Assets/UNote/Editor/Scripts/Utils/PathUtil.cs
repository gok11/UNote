using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace UNote.Editor
{
    internal static class PathUtil
    {
        #region Field

        private static bool s_initialized = false;
        private static bool s_isInstalledAsPackage = false;

        #endregion // Field

        #region Const

        private const string PackageIdentity = "com.mandarin-box.unote";

        #endregion // Const

        #region Property

        internal static bool Initialized => s_initialized;

        internal static bool IsInstalledAsPackage => s_initialized && s_isInstalledAsPackage;

        #endregion // Property

        #region Constructor

        [InitializeOnLoadMethod]
        internal static void Initialize()
        {
            ListRequest request = Client.List();
            EditorApplication.update += DetectIfUNoteIsPackage;
            
            void DetectIfUNoteIsPackage()
            {
                if (!request.IsCompleted)
                {
                    return;
                }
                
                EditorApplication.update -= DetectIfUNoteIsPackage;
                
                if (request.Status != StatusCode.Success)
                {
                    return;
                }
                
                s_isInstalledAsPackage = request.Result.Any(t => t.name == PackageIdentity);
                s_initialized = true;
            }
        }

        #endregion // Constructor

        #region Function

        private static string GetRootPath()
        {
            return IsInstalledAsPackage ? $"Packages/{PackageIdentity}" : $"Assets/UNote";
        }

        internal static string GetUxmlPath(string fileName)
        {
            return $"{GetRootPath()}/Editor/UXML/{fileName}";
        }

        internal static string GetUssPath(string fileName)
        {
            return $"{GetRootPath()}/Editor/USS/{fileName}";
        }

        internal static string GetTexturePath(string fileName)
        {
            return $"{GetRootPath()}/Editor/Texture/{fileName}";
        }

        internal static string GetScreenshotSavePath()
        {
            string folder = "Assets/UNote/NoteAssets/ScreenShots/";
            string nowStr = DateTime.Now.ToString("s").Replace(":", "_").Replace("-", "_");
            string fileName = $"{UNoteSetting.UserName}_{nowStr}.png";
            return Path.Combine(folder, fileName).FullPathToAssetPath();
        }

        #endregion // Function
    }
}
