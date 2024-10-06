using System.Text;
using UnityEngine;

namespace UNote.Editor
{
    /// <summary>
    /// String utils and extensions
    /// </summary>
    internal static class StringUtil
    {
        private static StringBuilder s_stringBuilder = new StringBuilder();

        /// <summary>
        /// Get cleared StringBuilder cache
        /// </summary>
        internal static StringBuilder StringBuilder
        {
            get
            {
                s_stringBuilder.Clear();
                return s_stringBuilder;
            }
        }

        /// <summary>
        /// Full path to Assets path for AssetDatabase API
        /// </summary>
        internal static string FullPathToAssetPath(this string absolutePath)
        {
            return absolutePath.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }

        /// <summary>
        /// String.IsNullOrEmpty()
        /// </summary>
        internal static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        
        /// <summary>
        /// String.IsNullOrWhiteSpace()
        /// </summary>
        internal static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
    }
}
