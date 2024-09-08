using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace UNote.Editor
{
    public static class PathUtil
    {
        private static bool s_initializeCompleted = false;
        private static bool s_isPackage = false;

        #region Constructor

        static PathUtil()
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
                
                s_isPackage = request.Result.Any(t => t.name == "com.mandarin-box.unote");
                s_initializeCompleted = true;
            }
        }

        #endregion // Constructor
        

    }
}
