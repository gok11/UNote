using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class AssetNoteAddWindow : EditorWindow
    {
        private void CreateGUI()
        {
            titleContent = new GUIContent("New Asset Note");
            
            // Label
            Label label = new Label("Bind Asset");
            label.style.SetMargin(2, 2, 2, 0);
            
            // Bind Asset
            ObjectField assetField = new ObjectField("Target Asset");
            assetField.allowSceneObjects = false;

            // Button
            Button addButton = new Button();
            addButton.text = "Add Asset Note";
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
            
            rootVisualElement.Add(label);
            rootVisualElement.Add(assetField);
            rootVisualElement.Add(addButton);
        }
    }
}
