using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UNote.Editor
{
    public class UNotePreferenceSettingsProvider : SettingsProvider
    {
        private const string SettingPath = "Preferences/UNote";
        
        [SettingsProvider]
        private static SettingsProvider CreateUNoteSettingProvider()
        {
            return new UNotePreferenceSettingsProvider(SettingPath, SettingsScope.User,
                new HashSet<string>(new[] { "UNote" }));
        }

        private UNotePreferenceSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            UNotePreferences preferences = UNotePreferences.instance;
            preferences.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            
            // Construct UI
            TextField userName = new TextField("User Name");
            userName.bindingPath = "m_userName";
            rootElement.Add(userName);

            userName.RegisterValueChangedCallback(evt =>
            {
                if (!evt.newValue.IsNullOrWhiteSpace())
                {
                    UNotePreferences.instance.Save();
                }
            });
            
            // Bind
            rootElement.Bind(new SerializedObject(preferences));
        }
    }
}
