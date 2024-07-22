using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UNote.Runtime;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    [InitializeOnLoad]
    public static class NoteInjector
    {
        private static FieldInfo s_trackerField = typeof(UnityEditor.Editor)
                .Assembly.GetType("UnityEditor.InspectorWindow")
                .GetField("m_Tracker", BindingFlags.NonPublic | BindingFlags.Instance);

        private static List<Object> s_targetTempList = new(64);
        private static List<Object> s_removeTempList = new(64);

        private static int s_intervalCounter = 0;
        private const int Interval = 2;
        
        static NoteInjector()
        {
            EditorApplication.update -= TryInjectNoteElement;
            EditorApplication.update += TryInjectNoteElement;

            s_intervalCounter = Interval;
        }

        static void TryInjectNoteElement()
        {
            s_intervalCounter = Mathf.Max(0, s_intervalCounter - 1);
            if (s_intervalCounter != 0)
            {
                return;
            }
            
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var window in windows)
            {
                if (window.GetType().FullName != "UnityEditor.InspectorWindow")
                {
                    continue;
                }
                
                VisualElement inspectorNoteEditor = window.rootVisualElement.Q(nameof(InspectorNoteEditor));
                if (inspectorNoteEditor != null)
                {
                    continue;
                }
                
                VisualElement inspectorElement = window.rootVisualElement.Q<InspectorElement>();
                if (inspectorElement == null)
                {
                    continue;
                }
                
                VisualElement parentElement = inspectorElement.parent;
                int insertIndex = parentElement.IndexOf(inspectorElement);
                
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
                
                s_targetTempList.Clear();
                foreach (var editor in activeEditors)
                {
                    s_targetTempList.Add(editor.target);
                }
                
                // Determine if this is prefab
                foreach (var editor in activeEditors)
                {
                    Object target = editor.target;
                    if (PrefabUtility.IsPartOfPrefabAsset(target))
                    {
                        inspectorNoteEditor = new InspectorNoteEditor(target);
                        parentElement.Insert(insertIndex, inspectorNoteEditor);
                        return;
                    }
                }
                
                // Determine if this is gameobject
                foreach (var editor in activeEditors)
                {
                    Object target = editor.target;
                    if (target is GameObject)
                    {
                        // TODO Not supported yet
                        parentElement.Insert(insertIndex, new VisualElement { name = nameof(InspectorNoteEditor) });

                        // inspectorNoteEditor = new InspectorNoteEditor(NoteType.Scene, target);
                        // parentElement.Insert(insertIndex, inspectorNoteEditor);
                        return;
                    }
                }
                
                // register objects except component
                foreach (var editor in activeEditors)
                {
                    Object target = editor.target;
                    if (target is not Component)
                    {
                        inspectorNoteEditor = new InspectorNoteEditor(target);
                        parentElement.Insert(insertIndex, inspectorNoteEditor);
                        return;
                    }
                }
            }
        }
    }
}
