using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    /// <summary>
    /// Inject note elements in inspector
    /// </summary>
    [InitializeOnLoad]
    public static class NoteInjector
    {
        static NoteInjector()
        {
            EditorApplication.update -= TryInjectNoteElement;
            EditorApplication.update += TryInjectNoteElement;
        }

        private static void TryInjectNoteElement()
        {
            // wait for initialize
            if (!Utility.Initialized)
            {
                return;
            }
            
            // inject impl
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var window in windows)
            {
                string typeName = window.GetType().FullName;
                bool isInspector = typeName == "UnityEditor.InspectorWindow";
                bool isPropertyEditor = typeName == "UnityEditor.PropertyEditor";
                
                if (!(isInspector || isPropertyEditor))
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
                
                UnityEditor.Editor[] activeEditors = ActiveEditorTracker.sharedTracker.activeEditors;
                if (activeEditors == null || activeEditors.Length == 0)
                {
                    continue;
                }
                
                // Determine if this is prefab
                foreach (var editor in activeEditors)
                {
                    Object target = editor.target;
                    if (PrefabUtility.IsPartOfPrefabAsset(target))
                    {
                        // Prefab
                        inspectorNoteEditor = new InspectorNoteEditor(target);
                        parentElement.Insert(insertIndex, inspectorNoteEditor);
                        return;
                    }
                    else if (target is GameObject)
                    {
                        // GameObject dummy
                        parentElement.Insert(insertIndex, new VisualElement { name = nameof(InspectorNoteEditor) });

                        // TODO GameObject support
                        // inspectorNoteEditor = new InspectorNoteEditor(NoteType.Scene, target);
                        // parentElement.Insert(insertIndex, inspectorNoteEditor);
                        return;
                    }
                    else if (target is not Component)
                    {
                        // Other Object except Component
                        inspectorNoteEditor = new InspectorNoteEditor(target);
                        parentElement.Insert(insertIndex, inspectorNoteEditor);
                        return;
                    }
                }
            }
        }
    }
}
