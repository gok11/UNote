using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    public class InspectorNoteEditor : VisualElement
    {
        private Foldout m_foldout;
        private VisualElement m_content;
        
        // Note list
        private VisualElement m_noteListBorder;
        private ScrollView m_noteList;
        private VisualElement m_footerElem;

        private NoteInputField m_noteInputField;
        
        public InspectorNoteEditor(NoteBase note, Object target)
        {
            name = nameof(InspectorNoteEditor);
            
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.InspectorNoteEditor
            );
            contentContainer.Add(tree.Instantiate());

            m_foldout = contentContainer.Q<Foldout>("Foldout");
            m_content = contentContainer.Q("Content");

            m_noteListBorder = contentContainer.Q("NoteListBorder");
            m_noteList = contentContainer.Q<ScrollView>("NoteList");

            m_noteInputField = new NoteInputField(note);
            m_noteInputField.style.marginLeft = 16;
            m_content.Add(m_noteInputField);
            
            EditorApplication.delayCall += () =>
            EditorApplication.delayCall += () =>
            {
                SetupNoteList(note.NoteType, target);
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
            });

            EditorUNoteManager.OnNoteAdded += note => SetupNoteList(note.NoteType, target);
            EditorUNoteManager.OnNoteDeleted += note => SetupNoteList(note.NoteType, target);
        }

        private void SetupNoteList(NoteType noteType, Object target)
        {
            m_noteList.contentContainer.Clear();

            m_noteList.visible = false;
            m_noteListBorder.style.display = DisplayStyle.None;
            
            m_footerElem = new VisualElement();
            m_footerElem.name = "footer_elem";
            m_footerElem.style.height = 0;
            m_noteList.Add(m_footerElem);

            // ターゲットと紐づくメモ一覧をリスト表示する
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
                }
            }
        }
    }
}
