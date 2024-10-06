using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    /// <summary>
    /// VisualElement extensions
    /// </summary>
    public class VisualElementUtil
    {
        /// <summary>
        /// Create drop area to convert asset path into guid
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
                        if (!parentTextField.value.IsNullOrEmpty())
                        {
                            parentTextField.value += "\n";
                        }
                        parentTextField.value += $"[unatt-guid:{guid}]";
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
        
        /// <summary>
        /// Show Generic menu for adding tag
        /// </summary>
        /// <param name="container"></param>
        internal static void ShowAddTagMenu(VisualElement container)
        {
            VisualElement tags = container.Q("Tags");

            List<UNoteTag> tempList = new();
            tags.Query<UNoteTag>().ToList(tempList);
            
            GenericMenu menu = new GenericMenu();

            List<UNoteTagData> tagList = UNoteSetting.TagList;
            foreach (var tagData in tagList)
            {
                // Overlap check
                if (tempList.Find(t => t.TagId == tagData.TagId) != null)
                {
                    continue;
                }
                
                // Add item
                menu.AddItem(new GUIContent(tagData.TagName), false, () =>
                {
                    tags.Add(new UNoteTag(tagData.TagId, true));
                });
            }
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Open Tag Setting"), false, ()=>
            {
                SettingsService.OpenProjectSettings("Project/UNote");
            });
            
            menu.ShowAsContext();
        }
    }
}
