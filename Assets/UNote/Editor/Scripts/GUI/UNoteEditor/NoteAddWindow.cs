using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class NoteAddWindow : EditorWindow
    {
        #region Field

        private VisualElement m_container;

        #endregion // Field

        #region Private Method

                private void OnEnable()
        {
            titleContent = new GUIContent("New Note");   
        }

        private void CreateGUI()
        {
            // Type
            EnumField noteTypeField = new EnumField("Note Type", NoteType.Project);
            noteTypeField.style.SetMargin(4, 4, 2, 4);
            
            m_container = new VisualElement();
            m_container.style.flexGrow = 1;
            
            noteTypeField.RegisterValueChangedCallback(t =>
            {
                m_container.Clear();
                
                switch ((NoteType)t.newValue)
                {
                    case NoteType.Project:
                        CreateProjectNoteGUI();
                        break;
                    
                    case NoteType.Asset:
                        CreateAssetNoteGUI();
                        break;
                }
            });
            
            rootVisualElement.Add(noteTypeField);
            rootVisualElement.Add(m_container);
            
            // for initial type
            CreateProjectNoteGUI();
        }

        private void CreateProjectNoteGUI()
        {
            // Button
            Button addButton = new Button();
            addButton.style.SetMargin(2, 4, 2, 4);
            addButton.text = "Add New Note";
            addButton.clicked += () =>
            {
                ProjectNote projectNote = EditorUNoteManager.AddNewProjectNote();
                EditorUNoteManager.SelectNote(projectNote);
                
                Close();
            };
            
            m_container.AddSpacer();
            m_container.Add(addButton);
        }

        private void CreateAssetNoteGUI()
        {
            // Bind Asset
            ObjectField assetField = new ObjectField("Bind Asset");
            assetField.style.SetMargin(2, 4, 2, 4);
            assetField.allowSceneObjects = false;
            
            // Button
            Button addButton = new Button();
            addButton.style.SetMargin(2, 4, 2, 4);
            addButton.text = "Add New Note";
            addButton.SetEnabled(false);
            addButton.clicked += () =>
            {
                string path = AssetDatabase.GetAssetPath(assetField.value);
                string guid = AssetDatabase.AssetPathToGUID(path);
                AssetNote assetNote = EditorUNoteManager.AddNewAssetNote(guid, "");
                EditorUNoteManager.SelectNote(assetNote);
                
                Close();
            };

            assetField.RegisterValueChangedCallback(t =>
            {
                addButton.SetEnabled(t.newValue);
            });
            
            m_container.Add(assetField);
            m_container.AddSpacer();
            m_container.Add(addButton);
        }

        #endregion // Private Method
        

    }
}
