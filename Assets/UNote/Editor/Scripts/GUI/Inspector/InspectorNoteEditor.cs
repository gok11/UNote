using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    public sealed class InspectorNoteEditor : VisualElement
    {
        private Foldout m_foldout;
        private Button m_openButton;
        private VisualElement m_content;
        
        // Note list
        private VisualElement m_noteListBorder;
        private ScrollView m_noteList;
        private VisualElement m_footerElem;

        private NoteInputField m_noteInputField;
        
        public InspectorNoteEditor(Object target)
        {
            name = nameof(InspectorNoteEditor);

            NoteType noteType = GetTargetNoteType(target);
            string noteId = GetBindId(noteType, target);
            
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.InspectorNoteEditor
            );
            contentContainer.Add(tree.Instantiate());

            m_foldout = contentContainer.Q<Foldout>("Foldout");
            m_openButton = contentContainer.Q<Button>("OpenButton");
            m_content = contentContainer.Q("Content");

            m_noteListBorder = contentContainer.Q("NoteListBorder");
            m_noteList = contentContainer.Q<ScrollView>("NoteList");

            m_noteInputField = new NoteInputField(noteType, noteId);
            m_noteInputField.style.marginLeft = 16;
            m_content.Add(m_noteInputField);
            
            EditorApplication.delayCall += () =>
            EditorApplication.delayCall += () =>
            {
                SetupNoteList(noteType, target);
            };

            m_foldout.RegisterValueChangedCallback(opened =>
            {
                m_content.style.display = opened.newValue ? DisplayStyle.Flex : DisplayStyle.None;

                if (m_footerElem != null && m_noteList.Contains(m_footerElem))
                {
                    EditorApplication.delayCall += () =>
                    {
                        m_noteList.ScrollTo(m_footerElem);
                    };
                }

                UserConfig.GetUNoteSetting().InspectorFoldoutOpened = opened.newValue;
            });

            bool foldout = UserConfig.GetUNoteSetting().InspectorFoldoutOpened;
            m_foldout.value = foldout;
            m_content.style.display = foldout ? DisplayStyle.Flex : DisplayStyle.None;
            
            m_openButton.SetEnabled(false);
            m_openButton.clicked += () =>
            {
                EditorWindow.GetWindow<UNoteEditor>();
                
                EditorUNoteManager.SelectCategory(noteType);
                
                switch (noteType)
                {
                    case NoteType.Asset:
                        string assetPath = AssetDatabase.GetAssetPath(target);
                        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                        NoteBase note = EditorUNoteManager.GetAssetLeafNoteListByNoteId(assetGuid).First();
                        EditorUNoteManager.SelectNote(note);
                        break;
                    
                    default:
                        throw new NotImplementedException();
                }
            };

            EditorUNoteManager.OnNoteAdded += _ => SetupNoteList(noteType, target);
            EditorUNoteManager.OnNoteDeleted += _ => SetupNoteList(noteType, target);
        }

        private NoteType GetTargetNoteType(Object target)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(target))
            {
                return NoteType.Asset;
            }

            // if (target is GameObject)
            // {
            //     return NoteType.Scene;
            // }

            return NoteType.Asset;
        }

        private string GetBindId(NoteType noteType, Object target)
        {
            switch (noteType)
            {
                case NoteType.Asset:
                    string path = AssetDatabase.GetAssetPath(target);
                    return AssetDatabase.AssetPathToGUID(path);
                
                default:
                    return null;
            }
        }
        
        private void SetupNoteList(NoteType noteType, Object target)
        {
            m_noteList.contentContainer.Clear();
            m_openButton.SetEnabled(false);

            m_noteList.visible = false;
            m_noteListBorder.style.display = DisplayStyle.None;
            
            m_footerElem = new VisualElement();
            m_footerElem.name = "footer_elem";
            m_footerElem.style.height = 0;
            m_noteList.Add(m_footerElem);

            // Display note list associated with a target
            switch (noteType)
            {
                case NoteType.Asset:
                    string assetPath = AssetDatabase.GetAssetPath(target);
                    string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                    
                    foreach (var leafNote in EditorUNoteManager.GetAssetLeafNoteListByNoteId(assetGuid))
                    {
                        m_noteList.Insert(m_noteList.childCount - 1, new UNoteEditorContentElem(leafNote));
                    }
                    break;
                
                default:
                    throw new NotImplementedException();
            }
            
            if (m_noteList != null)
            {
                EditorApplication.delayCall += () =>
                {
                    if (m_noteList.Contains(m_footerElem))
                    {
                        m_noteList.ScrollTo(m_footerElem);   
                    }
                    m_noteList.visible = true;
                };
                
                if (m_noteList.childCount > 1)
                {
                    m_noteListBorder.style.display = DisplayStyle.Flex;
                    m_openButton.SetEnabled(true);
                }
            }
        }
    }
}
