using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class VisualElementUtil
    {
        /// <summary>
        /// Create drop area for asset path to guid
        /// </summary>
        /// <returns></returns>
        internal static void CreateDropAreaElem(TextField parentTextField)
        {
            VisualElement dropAreaElement = new VisualElement();
            
            dropAreaElement.RegisterCallback<DragEnterEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                evt.StopPropagation();
            });
            
            dropAreaElement.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                evt.StopPropagation();
            });
            
            dropAreaElement.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();

                string[] paths = DragAndDrop.paths;
                foreach (var path in paths)
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        parentTextField.value += $"\n[unref-guid:{guid}]";
                    }
                }
                
                evt.StopPropagation();
            });

            parentTextField.Add(dropAreaElement);
            dropAreaElement.StretchToParentSize();
            
            parentTextField.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                dropAreaElement.StretchToParentSize();
            });
        }
    }
}
