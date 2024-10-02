using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class UNoteProjectSettingsProvider : SettingsProvider
    {
        private const string SettingPath = "Project/UNote";

        private VisualElement m_rootElement;
        
        [SettingsProvider]
        private static SettingsProvider CreateUNoteSettingProvider()
        {
            return new UNoteProjectSettingsProvider(SettingPath, SettingsScope.Project,
                new HashSet<string>(new[] { "UNote" }));
        }

        private UNoteProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_rootElement = rootElement;
            
            UNoteProjectSettings settings = UNoteProjectSettings.instance;
            settings.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;

            SerializedObject serializedObject = settings.SerializedObject;

            ListView listView = new ListView
            {
                bindingPath = "m_tagList",
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showFoldoutHeader = true,
                headerTitle = "Tags",
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeItem = MakeItem,
            };
            listView.reorderable = true;
            
            // List callback
            listView.itemsAdded += indices =>
            {
                // Initialize added item
                foreach (var idx in indices)
                {
                    settings.m_tagList[idx].Initialize();
                }
            };
            
            listView.itemsRemoved += _ =>
            {
                SaveSettings();
            };
            
            rootElement.Add(listView);
            
            rootElement.Bind(serializedObject);
        }

        /// <summary>
        /// Make each line gui item
        /// </summary>
        /// <returns></returns>
        private BindableElement MakeItem()
        {
            BindableElement root = new BindableElement { style = { flexDirection = FlexDirection.Row } };

            TextField textField = new TextField("Tag Name")
            {
                bindingPath = "m_tagName",
                style = { flexGrow = 1 }
            };
            textField.isDelayed = true;
            textField.RegisterValueChangedCallback(_ => SaveSettings());
            root.Add(textField);

            ColorField colorField = new ColorField("Color")
            {
                bindingPath = "m_color",
                style = { width = 160f }
            };
            colorField.RegisterValueChangedCallback(_ =>
            {
                SaveSettings();
            });
            colorField.Q<Label>().style.minWidth = 40;
            root.Add(colorField);
                    
            return root;
        }
        
        private void SaveSettings()
        {
            UNoteProjectSettings.instance.Save();
            
            // Update const file
            NoteTagConstGenerator.Generate();
        }
    }
}
