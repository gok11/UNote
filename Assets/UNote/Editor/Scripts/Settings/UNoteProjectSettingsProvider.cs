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
            UNoteProjectSettings settings = UNoteProjectSettings.instance;
            settings.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;

            ListView listView = new ListView
            {
                bindingPath = "m_tagList",
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showFoldoutHeader = true,
                headerTitle = "Tags",
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeItem = () =>
                {
                    BindableElement root = new BindableElement { style = { flexDirection = FlexDirection.Row } };

                    TextField textField = new TextField("Tag Name")
                    {
                        bindingPath = "m_tagName",
                        style = { flexGrow = 1 }
                    };
                    root.Add(textField);

                    ColorField colorField = new ColorField("Color")
                    {
                        bindingPath = "m_color",
                        style = { width = 160f }
                    };
                    colorField.Q<Label>().style.minWidth = 40;
                    root.Add(colorField);
                    
                    return root;
                }
            };
            listView.itemsAdded += indices =>
            {
                foreach (var idx in indices)
                {
                    // Initialize TagID
                    settings.m_tagList[idx].TagId = Guid.NewGuid().ToString();
                    Debug.Log(settings.m_tagList[idx].TagId );
                }
            };
            listView.reorderable = true;
            
            rootElement.Add(listView);
            
            rootElement.Bind(new SerializedObject(settings));
        }
    }
}
