using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class SettingsRegister
    {
        [SettingsProvider]
        private static SettingsProvider CreateUNoteSettingProvider()
        {
            var provider = new SettingsProvider("Preferences/", SettingsScope.User)
            {
                label = "UNote",
                guiHandler = (searchContext) =>
                {
                    ++EditorGUI.indentLevel;

                    UNoteSetting.UserName = EditorGUILayout.TextField("Editor Name", UNoteSetting.UserName);
                    if (string.IsNullOrEmpty(UNoteSetting.UserName))
                    {
                        UNoteSetting.UserName = Environment.UserName;
                    }

                    --EditorGUI.indentLevel;
                },
                keywords = new HashSet<string>(new[] { "UNote" })
            };
            return provider;
        }
    }
}
