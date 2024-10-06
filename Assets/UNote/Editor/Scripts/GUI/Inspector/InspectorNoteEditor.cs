using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;
using Object = UnityEngine.Object;

namespace UNote.Editor
{
    /// <summary>
    /// NoteEditor part VisualElement. Note editor in Inspector
    /// </summary>
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
            (string bindId, string objectId) ids = GetBindId(noteType, target);

            // Get VisualElements
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.InspectorNoteEditor
            );
            contentContainer.Add(tree.Instantiate());

            m_foldout = contentContainer.Q<Foldout>("Foldout");
            m_openButton = contentContainer.Q<Button>("OpenButton");
            m_content = contentContainer.Q("Content");

            m_noteListBorder = contentContainer.Q("NoteListBorder");
            m_noteList = contentContainer.Q<ScrollView>("NoteList");

            m_noteInputField = new NoteInputField(noteType, ids.bindId, ids.objectId);
            m_noteInputField.style.marginLeft = 16;
            m_content.Add(m_noteInputField);
            
            // Load messages
            EditorApplication.delayCall += () =>
            EditorApplication.delayCall += () =>
            {
                SetupMessageList(noteType, target);
            };

            // Register callback
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

                UNoteSetting.InspectorFoldoutOpened = opened.newValue;
            });

            // Foldout
            bool foldout = UNoteSetting.InspectorFoldoutOpened;
            m_foldout.value = foldout;
            m_content.style.display = foldout ? DisplayStyle.Flex : DisplayStyle.None;

            // NoteEditor for current target open button
            m_openButton.Q("Visual").style.backgroundImage =
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathUtil.GetTexturePath("editor.png"));
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
                        List<AssetNote> noteList = EditorUNoteManager.GetAssetNoteListByGuid(assetGuid);
                        EditorUNoteManager.SelectNote(noteList.FirstOrDefault());
                        break;
                    
                    default:
                        throw new NotImplementedException();
                }
            };

            // Event callback
            EditorUNoteManager.OnNoteAdded += _ => SetupMessageList(noteType, target);
            EditorUNoteManager.OnNoteDeleted += _ => SetupMessageList(noteType, target);
        }

        /// <summary>
        /// Get NoteType for specified target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get ID for specified target
        /// </summary>
        /// <param name="noteType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private (string, string) GetBindId(NoteType noteType, Object target)
        {
            switch (noteType)
            {
                case NoteType.Asset:
                    string path = AssetDatabase.GetAssetPath(target);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    string noteId = EditorUNoteManager.GetAssetNoteListByGuid(guid).FirstOrDefault()?.NoteId;
                    return (noteId, guid);
                
                default:
                    return (null, null);
            }
        }
        
        /// <summary>
        /// Load messages
        /// </summary>
        /// <param name="noteType"></param>
        /// <param name="target"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SetupMessageList(NoteType noteType, Object target)
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

                    // TODO
                    AssetNote assetNote = EditorUNoteManager.GetAssetNoteListByGuid(assetGuid).FirstOrDefault();
                    if (assetNote != null)
                    {
                        foreach (var noteMessage in EditorUNoteManager.GetAssetNoteMessageListByNoteId(assetNote.NoteId))
                        {
                            m_noteList.Insert(m_noteList.childCount - 1, new UNoteEditorContentElem(noteMessage));
                        }   
                    }
                    break;
                
                default:
                    throw new NotImplementedException();
            }
            
            // Scroll to latest message
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
