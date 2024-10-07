using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    /// <summary>
    /// NoteEditor part VisualElement. Custom query list item.
    /// </summary>
    public class CustomNoteQueryElem : VisualElement
    {
        private NoteQuery m_noteQuery;
        private Button m_contextButton;
        
        public CustomNoteQueryElem(UNoteEditorLeftPane leftPane, NoteQuery noteQuery)
        {
            m_noteQuery = noteQuery;
            
            VisualTreeAsset noteQueryTemplateAsset =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath.NoteQueryTemplate);
            TemplateContainer noteQueryTemplate = noteQueryTemplateAsset.Instantiate();
            
            contentContainer.Add(noteQueryTemplate);
            contentContainer.Q<Label>("Label").text = noteQuery.QueryName;
            m_contextButton = contentContainer.Q<Button>("ContextButton");

            m_contextButton.visible = false;
            
            // Register event
            RegisterCallback<MouseDownEvent>(evt =>
            {
                // Update query
                if (EditorUNoteManager.CurrentNoteQuery != m_noteQuery)
                {
                    EditorUNoteManager.SetNoteQuery(m_noteQuery);
                    leftPane.SelectQueryElem(m_noteQuery);
                    
                    UNoteSetting.LastSelectedQuery = m_noteQuery;
                }
                
                switch (evt.button)
                {
                    // Right click
                    case 1:
                        ShowContextMenu();
                        break;
                }
                
                evt.StopPropagation();
            });
            
            contentContainer.RegisterCallback<MouseEnterEvent>(_ =>
            {
                m_contextButton.visible = true;
            });
            
            contentContainer.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                m_contextButton.visible = false;
            });

            m_contextButton.clicked += () =>
            {
                // Update query
                if (EditorUNoteManager.CurrentNoteQuery != m_noteQuery)
                {
                    EditorUNoteManager.SetNoteQuery(m_noteQuery);
                    leftPane.SelectQueryElem(m_noteQuery);
                }

                ShowContextMenu();
            };
        }

        /// <summary>
        /// Edit query menu
        /// </summary>
        private void ShowContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(
                new GUIContent("Rename"),
                false,
                () =>
                {
                    UNoteEditor.CenterPane.EnableChangeQueryNameMode();
                }
            );
            
            menu.AddItem(
                new GUIContent("Delete"),
                false,
                () =>
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Do you want to delete this query?", "OK", "Cancel"))
                    {
                        EditorUNoteManager.DeleteQuery(m_noteQuery);
                    }
                }
            );
            
            menu.ShowAsContext();
        }
    }
}
