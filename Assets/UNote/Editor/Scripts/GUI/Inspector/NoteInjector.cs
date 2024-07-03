using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    [InitializeOnLoad]
    public static class NoteInjector
    {
        private static FieldInfo s_trackerField = typeof(UnityEditor.Editor)
                .Assembly.GetType("UnityEditor.InspectorWindow")
                .GetField("m_Tracker", BindingFlags.NonPublic | BindingFlags.Instance);

        private static Dictionary<EditorWindow, VisualElement> s_elemDict = new();
        
        static NoteInjector()
        {
            EditorApplication.update -= TryInjectNoteElement;
            EditorApplication.update += TryInjectNoteElement;
        }

        static void TryInjectNoteElement()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var window in windows)
            {
                if (window.GetType().FullName != "UnityEditor.InspectorWindow")
                {
                    continue;
                }

                VisualElement inspectorNoteEditor = window.rootVisualElement.Q("InspectorNoteEditor");
                if (inspectorNoteEditor != null)
                {
                    continue;
                }

                VisualElement editorsList =
                    window.rootVisualElement.Q<VisualElement>(null, "unity-inspector-editors-list");
                
                var tracker = s_trackerField.GetValue(window) as ActiveEditorTracker;
                if (tracker == null)
                {
                    continue;
                }
                
                var activeEditors = tracker.activeEditors;
                if (activeEditors == null || activeEditors.Length == 0)
                {
                    continue;
                }
                
                // プレハブかを最初に判別
                foreach (var editor in activeEditors)
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(editor.target))
                    {
                        Debug.Log("prafab");

                        inspectorNoteEditor = new InspectorNoteEditor();
                        inspectorNoteEditor.name = "InspectorNoteEditor";
                        window.rootVisualElement.Insert(0, inspectorNoteEditor);

                        return;
                    }
                }
                
                // ゲームオブジェクトかを次に判別
                foreach (var editor in activeEditors)
                {
                    if (editor.target is GameObject)
                    {
                        Debug.Log("game");
                        inspectorNoteEditor = new InspectorNoteEditor();
                        inspectorNoteEditor.name = "InspectorNoteEditor";
                        window.rootVisualElement.Insert(0, inspectorNoteEditor);

                        return;
                    }
                }
                
                Debug.Log("general");
                inspectorNoteEditor = new InspectorNoteEditor();
                inspectorNoteEditor.name = "InspectorNoteEditor";
                window.rootVisualElement.Insert(0, inspectorNoteEditor);
            }
            
        }

        private static VisualElement CreatePrefabNoteElem(VisualElement parent)
        {
            VisualElement prefabNoteElem =  new VisualElement()
            {
                name = "PrefabElem"
            };
            parent.Add(prefabNoteElem);
            return prefabNoteElem;
        }

        private static VisualElement CreateGameObjectNoteElem(VisualElement parent)
        {
            VisualElement gameObjectNote = new VisualElement()
            {
                name = "GoElem"
            };
            parent.Add(gameObjectNote);
            return gameObjectNote;
        }

        private static VisualElement CreateGeneralNoteElem(VisualElement parent)
        {
            VisualElement generalNoteElem = new VisualElement()
            {
                name = "GeneralElem"
            };
            parent.Add(generalNoteElem);
            return generalNoteElem;
        }
    }
}
