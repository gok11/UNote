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
                
                // VisualElement editorsList = window.rootVisualElement.Q<VisualElement>(null, "unity-inspector-editors-list");
                VisualElement inspectorElement = window.rootVisualElement.Q<InspectorElement>();
                if (inspectorElement == null)
                {
                    continue;
                }

                VisualElement inspectorNoteEditor = window.rootVisualElement.Q("InspectorNoteEditor");
                if (inspectorNoteEditor != null)
                {
                    continue;
                }
                
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
                
                // プレハブかを最初に判別
                foreach (var editor in activeEditors)
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(editor.target))
                    {
                        inspectorNoteEditor = new InspectorNoteEditor(NoteType.Asset, editor.target);
                        inspectorElement.Insert(1, inspectorNoteEditor);
                        return;
                    }
                }
                
                // ゲームオブジェクトかを次に判別
                foreach (var editor in activeEditors)
                {
                    if (editor.target is GameObject)
                    {
                        inspectorNoteEditor = new InspectorNoteEditor(NoteType.Sceene, editor.target);
                        inspectorElement.Insert(1, inspectorNoteEditor);
                        return;
                    }
                }
                
                // コンポーネントは除外しつつ登録 (DefaultAsset等)
                foreach (var editor in activeEditors)
                {
                    if (editor.target is not Component)
                    {
                        inspectorNoteEditor = new InspectorNoteEditor(NoteType.Asset, editor.target);
                        inspectorElement.Insert(1, inspectorNoteEditor);
                        return;
                    }
                }
            }
        }
    }
}
