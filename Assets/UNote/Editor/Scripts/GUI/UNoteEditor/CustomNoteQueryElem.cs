using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class CustomNoteQueryElem : VisualElement
    {
        private NoteQuery m_noteQuery;
        
        public CustomNoteQueryElem(UNoteEditorLeftPane leftPane, NoteQuery noteQuery)
        {
            m_noteQuery = noteQuery;
            
            VisualTreeAsset noteQueryTemplateAsset =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.NoteQueryTemplate);
            TemplateContainer noteQueryTemplate = noteQueryTemplateAsset.Instantiate();
            
            contentContainer.Add(noteQueryTemplate);

            // Register event
            RegisterCallback<MouseDownEvent>(evt =>
            {
                // Left click
                if (evt.button == 0)
                {
                    EditorUNoteManager.SetNoteQuery(m_noteQuery);
                    leftPane.SelectQueryElem(m_noteQuery);
                    evt.StopPropagation();
                }
                
                // Right click
                if (evt.button == 1)
                {
                    
                }
            });
        }
    }
}
