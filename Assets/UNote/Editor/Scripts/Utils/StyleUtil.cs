using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public static class StyleUtil
    {
        public static readonly Color UnselectColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        public static readonly Color SelectColor = new Color(0.1725f, 0.3647f, 0.5294f, 1.0f);
        public static readonly Color GrayedTextColor = new Color(0.6f, 0.6f, 0.6f);

        public static void SetMargin(this IStyle style, float margin)
        {
            style.marginTop = style.marginBottom = style.marginLeft = style.marginRight = margin;
        }

        public static void SetMargin(this IStyle style,
            float marginTop, float marginBottom, float marginLeft, float marginRight)
        {
            style.marginTop = marginTop;
            style.marginBottom = marginBottom;
            style.marginLeft = marginLeft;
            style.marginRight = marginRight;
        }

        public static void SetPadding(this IStyle style, float padding)
        {
            style.paddingTop = style.paddingBottom = style.paddingRight = style.paddingLeft = padding;
        }
        
        public static void SetPadding(this IStyle style,
            float paddingTop, float paddingBottom, float paddingRight, float paddingLeft)
        {
            style.paddingTop = paddingTop;
            style.paddingBottom = paddingBottom;
            style.paddingRight = paddingRight;
            style.paddingLeft = paddingLeft;
        }

        public static void SetBorderWidth(this IStyle style, float width)
        {
            style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = width;
        }
    }
}
