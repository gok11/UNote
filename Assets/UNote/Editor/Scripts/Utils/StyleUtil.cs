using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    /// <summary>
    /// Constants and extensions for style
    /// </summary>
    internal static class StyleUtil
    {
        internal static readonly Color UnselectColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        internal static readonly Color SelectColor = new Color(0.1725f, 0.3647f, 0.5294f, 1.0f);
        internal static readonly Color GrayColor = new Color(0.7f, 0.7f, 0.7f);
        internal static readonly Color BlackTextColor = new Color(0.11f, 0.11f, 0.11f);
        internal static readonly Color WhiteTextColor = new Color(0.75f, 0.75f, 0.75f);
        internal static readonly Color TodoInitColor = new Color(0.43f, 0.73f, 1.0f);
        internal static readonly Color BugInitColor = new Color(0.72f, 0.08f, 0.11f);
        internal static readonly Color GrayBorderColor = new Color(0.30f, 0.30f, 0.30f);
        
        private static Texture2D s_archiveIcon;
        private static Texture2D s_favoriteIcon;
        
        internal static Texture2D ArchiveIcon => s_archiveIcon;
        internal static Texture2D FavoriteIcon => s_favoriteIcon;
        
        static StyleUtil()
        {
            s_archiveIcon = EditorGUIUtility.IconContent("d_Package Manager").image as Texture2D;
            s_favoriteIcon = EditorGUIUtility.IconContent("d_Favorite").image as Texture2D;
        }
        
        /// <summary>
        /// Set IStyle.margin.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="margin"></param>
        internal static void SetMargin(this IStyle style, float margin)
        {
            style.marginTop = style.marginBottom = style.marginLeft = style.marginRight = margin;
        }

        /// <summary>
        /// Set IStyle.marginTop/Right/Bottom/Left
        /// </summary>
        /// <param name="style"></param>
        /// <param name="marginTop"></param>
        /// <param name="marginRight"></param>
        /// <param name="marginBottom"></param>
        /// <param name="marginLeft"></param>
        internal static void SetMargin(this IStyle style,
            float marginTop, float marginRight, float marginBottom, float marginLeft)
        {
            style.marginTop = marginTop;
            style.marginRight = marginRight;
            style.marginBottom = marginBottom;
            style.marginLeft = marginLeft;
        }

        /// <summary>
        /// Set IStyle.padding.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="padding"></param>
        internal static void SetPadding(this IStyle style, float padding)
        {
            style.paddingTop = style.paddingBottom = style.paddingRight = style.paddingLeft = padding;
        }
        
        /// <summary>
        /// Set IStyle.paddingTop/Right/Bottom/Left
        /// </summary>
        /// <param name="style"></param>
        /// <param name="paddingTop"></param>
        /// <param name="paddingRight"></param>
        /// <param name="paddingBottom"></param>
        /// <param name="paddingLeft"></param>
        internal static void SetPadding(this IStyle style,
            float paddingTop, float paddingRight, float paddingBottom, float paddingLeft)
        {
            style.paddingTop = paddingTop;
            style.paddingRight = paddingRight;
            style.paddingBottom = paddingBottom;
            style.paddingLeft = paddingLeft;
        }

        /// <summary>
        /// Set IStyle.borderWidth.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="width"></param>
        internal static void SetBorderWidth(this IStyle style, float width)
        {
            style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = width;
        }

        /// <summary>
        /// Set IStyle.borderColor.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="color"></param>
        internal static void SetBorderColor(this IStyle style, Color color)
        {
            style.borderTopColor = style.borderRightColor = 
                style.borderBottomColor = style.borderLeftColor = color;
        }
    }
}
