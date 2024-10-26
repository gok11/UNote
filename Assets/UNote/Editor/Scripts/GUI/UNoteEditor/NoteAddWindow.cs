using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// NoteEditor part VisualElement. New note add window.
    /// </summary>
    public class NoteAddWindow : EditorWindow
    {
        private VisualElement m_container;

        private void OnEnable()
        {
            titleContent = new GUIContent("New Note");
            minSize = maxSize = new(270, 100);
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
                    
                    case NoteType.Scene:
                        CreateSceneNoteGUI();
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
                AssetNote assetNote = EditorUNoteManager.AddNewAssetNote(guid);
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
        
        /// <summary>
        /// Scene note add GUI
        /// </summary>
        private void CreateSceneNoteGUI()
        {
            // Button
            Button addButton = new Button();
            addButton.style.SetMargin(2, 4, 2, 4);
            addButton.text = "Add New Note";
            addButton.clicked += () =>
            {
                SceneNote sceneNote = EditorUNoteManager.AddNewSceneNote();
                EditorUNoteManager.SelectNote(sceneNote);
                
                Close();
            };

            HelpBox helpBox = new HelpBox("Scene note can be created on saved scene.", HelpBoxMessageType.Warning);
            
            m_container.AddSpacer();
            m_container.Add(helpBox);
            m_container.Add(addButton);

            // Allow notes to be created only in saved scenes
            EnableSceneNoteGUI(EditorSceneManager.GetActiveScene());
            
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            EditorSceneManager.activeSceneChanged -= OnActiveSceneChanged;
            
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            EditorSceneManager.activeSceneChanged += OnActiveSceneChanged;            
            
            void OnActiveSceneChanged(Scene _, Scene activeScene)
            {
                EnableSceneNoteGUI(activeScene);
            }

            void EnableSceneNoteGUI(Scene scene)
            {
                bool isValidScene = !string.IsNullOrEmpty(scene.path);
                helpBox.style.display = isValidScene ? DisplayStyle.None : DisplayStyle.Flex;
                addButton.SetEnabled(isValidScene);
            }
        }
    }
}
