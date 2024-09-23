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
            contentContainer.Q<Label>("Label").text = noteQuery.QueryName;

            // Register event
            RegisterCallback<MouseDownEvent>(evt =>
            {
                // Update query
                if (EditorUNoteManager.CurrentNoteQuery != m_noteQuery)
                {
                    EditorUNoteManager.SetNoteQuery(m_noteQuery);
                    leftPane.SelectQueryElem(m_noteQuery);
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
        }

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
                        CustomQueryContainer container = CustomQueryContainer.Get();
                        int sourceIndex = container.NoteQueryList.FindIndex(t => t.QueryID == m_noteQuery.QueryID);
                        if (sourceIndex >= 0)
                        {
                            container.NoteQueryList.RemoveAt(sourceIndex);   
                        }

                        UNoteEditorLeftPane leftPane = UNoteEditor.LeftPane;
                        leftPane.LoadCustomQuery();
                
                        if (container.NoteQueryList.Count > 0)
                        {
                            EditorUNoteManager.SetNoteQuery(container.NoteQueryList[0]);
                        }
                        else
                        {
                            leftPane.SetDefaultQuery();
                        }
                
                        container.Save();
                    }
                }
            );
            
            menu.ShowAsContext();
        }
    }
}
