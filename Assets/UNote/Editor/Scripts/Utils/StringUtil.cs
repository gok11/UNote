using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UNote.Editor
{
    public static class StringUtil
    {
        #region Static Field

        private static StringBuilder s_stringBuilder = new StringBuilder();

        #endregion // Static Field

        #region Static Method

        public static StringBuilder StringBuilder
        {
            get
            {
                s_stringBuilder.Clear();
                return s_stringBuilder;
            }
        }

        public static string FullPathToAssetPath(this string absolutePath)
        {
            return absolutePath.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        }

        #endregion // Static Method
    }
}
