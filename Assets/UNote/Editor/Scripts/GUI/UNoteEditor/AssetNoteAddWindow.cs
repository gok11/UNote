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
            // Label
            Label label = new Label("Bind Asset");
            
            // Bind Asset
            ObjectField assetField = new ObjectField("Target Asset");
            assetField.allowSceneObjects = false;

            // Button
            Button addButton = new Button();
            addButton.text = "Add Asset Note";
            addButton.SetEnabled(false);
            addButton.clicked += () =>
            {
                AssetNote assetNote = EditorUNoteManager.AddOrGetAssetNote(assetField.value);
                EditorUNoteManager.SelectRoot(assetNote);
                
                UNoteEditor noteEditor = GetWindow<UNoteEditor>();
                noteEditor.CenterPane.SetupListItems();
                noteEditor.RightPane.SetupNoteList();
                
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
