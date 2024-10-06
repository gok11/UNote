using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    /// <summary>
    /// Auto scaled image display visual element
    /// </summary>
    public class FlexibleImageElem : VisualElement
    {
        private double m_lastCalcTime;
        private const double DebounceDelay = 0.016;
        
        public FlexibleImageElem(Texture2D tex, VisualElement parent)
        {
            CalcSize(tex.width);
            
            // Register callback
            parent.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                // bounce check
                double currentTime = Time.realtimeSinceStartupAsDouble;
                if (currentTime - m_lastCalcTime < DebounceDelay)
                {
                    return;
                }
                
                float width = evt.newRect.width;
                float maxWidth = width - 16;
                    
                CalcSize(maxWidth);

                m_lastCalcTime = currentTime;
            });

            // Calc size and resize
            void CalcSize(float maxWidth)
            {
                if (maxWidth <= 0)
                {
                    return;
                }
                
                float width = tex.width;
                float height = tex.height;
            
                if (width > maxWidth)
                {
                    float ratio = maxWidth / width;

                    width = maxWidth;
                    height *= ratio;
                }
            
                style.backgroundImage = tex;
                style.width = width;
                style.height = height;
            }
        }
    }
}
