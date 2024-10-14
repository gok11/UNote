using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// NoteEditor part VisualElement. Center pane list items
    /// </summary>
    internal class UNoteEditorListItem : VisualElement
    {
        private VisualElement m_parentContainer;

        private NoteBase m_note;

        private VisualElement m_archvieLabel;
        private VisualElement m_favoriteLabel;
        private VisualElement m_tagLine;
        private Label m_nameLabel;
        private Label m_noteContentLabel;
        private Button m_contextButton;
        private VisualElement m_noteListItem;

        internal NoteBase BindNote => m_note;

        internal UNoteEditorListItem(UNoteEditor noteEditor)
        {
            // VisualElements
            VisualTreeAsset listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteListItem
            );
            TemplateContainer template = listItem.CloneTree();

            m_archvieLabel = template.Q<VisualElement>("ArchiveLabel");
            m_favoriteLabel = template.Q<VisualElement>("FavoriteLabel");
            m_tagLine = template.Q("TagLine");
            m_nameLabel = template.Q<Label>("Name");
            m_noteContentLabel = template.Q<Label>("ContentLine");
            m_contextButton = template.Q<Button>("ContextButton");

            contentContainer.Add(template);

            m_noteListItem = contentContainer.Q("NoteListItem");

            // Style
            m_contextButton.visible = false;

            m_archvieLabel.style.backgroundImage = StyleUtil.ArchiveIcon;
            m_favoriteLabel.style.backgroundImage = StyleUtil.FavoriteIcon;

            // Event callbacks
            EditorUNoteManager.OnNoteFavoriteChanged += note =>
            {
                if (note == m_note)
                {
                    m_favoriteLabel.style.display = note.IsFavorite() ? DisplayStyle.Flex : DisplayStyle.None;
                }
            };
            
            EditorUNoteManager.OnNoteArchived += note =>
            {
                if (note == m_note)
                {
                    m_archvieLabel.style.display = note.Archived ? DisplayStyle.Flex : DisplayStyle.None;   
                }
            };
            
            // Handle mouse event
            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (EditorUNoteManager.CurrentNote != m_note)
                {
                    EditorUNoteManager.SelectNote(m_note);
                }

                evt.StopPropagation();
            });
        }

        internal void Setup(NoteBase note)
        {
            m_note = note;

            m_nameLabel.text = note.NoteName;

            // Note tag label
            foreach (var tag in NoteTagsUtl.ValidTags)
            {
                if (EditorUNoteManager.CheckTagInMessages(note, tag))
                {
                    m_tagLine.Add(CreateTagLineElem(tag));
                }
            }
            
            // Icon
            m_favoriteLabel.style.display = note.IsFavorite() ? DisplayStyle.Flex : DisplayStyle.None;
            m_archvieLabel.style.display = note.Archived ? DisplayStyle.Flex : DisplayStyle.None;

            // Label
            switch (note.NoteType)
            {
                case NoteType.Project:
                {
                    ProjectNoteMessage message = EditorUNoteManager
                        .GetProjectNoteMessageListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.CreatedDate)
                        .FirstOrDefault();

                    m_noteContentLabel.text = message
                        ?.NoteContent.Replace("\r", " ")
                        .Replace("\n", " ");

                    RegisterMouseEvent();
                    break;
                } 

                case NoteType.Asset:
                {
                    AssetNoteMessage message = EditorUNoteManager
                        .GetAssetNoteMessageListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.CreatedDate)
                        .FirstOrDefault();

                    m_noteContentLabel.text = message
                        ?.NoteContent.Replace("\r", " ")
                        .Replace("\n", " ");
                    
                    // TODO
                    RegisterMouseEvent();
                    break;
                }
            }
            Focus();
        }

        /// <summary>
        /// GUI event
        /// </summary>
        private void RegisterMouseEvent()
        {
            // Handle mouse event
            contentContainer.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (EditorUNoteManager.CurrentNote != m_note)
                {
                    EditorUNoteManager.SelectNote(m_note);
                }

                switch (evt.button)
                {
                    case 1:
                        ShowContextMenu();
                        break;
                }

                evt.StopPropagation();
            });

            contentContainer.RegisterCallback<MouseEnterEvent>(_ =>
            {
                m_contextButton.visible = true;
            });

            contentContainer.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                m_contextButton.visible = false;
            });

            // Handle button event
            m_contextButton.clicked += ShowContextMenu;
        }

        /// <summary>
        /// Create tag line visual element
        /// </summary>
        private VisualElement CreateTagLineElem(NoteTags tag)
        {
            VisualElement tagElem = new VisualElement();
            tagElem.style.backgroundImage = Texture2D.whiteTexture;
            tagElem.style.unityBackgroundImageTintColor = tag.ToColor();
            tagElem.style.width = 18;
            tagElem.style.height = 1;
            tagElem.style.marginRight = 2;
            return tagElem;
        }
        
        /// <summary>
        /// Set background color for item
        /// </summary>
        /// <param name="select"></param>
        internal void SetBackgroundColor(bool select)
        {
            m_noteListItem.style.backgroundColor = select ?
                StyleUtil.SelectColor : StyleUtil.UnselectColor;
        }

        /// <summary>
        /// Show context menu for editing note status
        /// </summary>
        private void ShowContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            bool isOwnNote = m_note.Author == UNoteSetting.UserName;
            
            // Favorite
            bool isFavorite = m_note.IsFavorite();
            string favoriteLabel = isFavorite ? "Unfavorite" : "Favorite";
            menu.AddItem(
                new GUIContent(favoriteLabel),
                false,
                () =>
                {
                    EditorUNoteManager.ToggleFavorite(m_note);
                }
            );
            
            menu.AddSeparator("");

            menu.AddItem(
                new GUIContent("Copy ID"),
                false,
                () =>
                {
                    EditorGUIUtility.systemCopyBuffer = $"nid:{m_note.NoteId}";
                }
            );
            
            // Rename
            if (m_note.NoteType == NoteType.Project && isOwnNote)
            {
                menu.AddItem(
                    new GUIContent("Rename"),
                    false,
                    () =>
                    {
                        UNoteEditor.RightPane.EnableChangeTitleMode();
                    }
                );
                
                menu.AddSeparator("");
            }

            // Archive, Delete
            if (isOwnNote)
            {
                string archiveLabel = m_note.Archived ? "Unarchived" : "Archive";
                menu.AddItem(
                    new GUIContent(archiveLabel),
                    false,
                    () =>
                    {
                        EditorUNoteManager.ToggleArchived(m_note);
                    });
            
                menu.AddItem(
                    new GUIContent("Delete"),
                    false,
                    () =>
                    {
                        if (EditorUtility.DisplayDialog("Confirm", "Do you want to delete this note?", "OK", "Cancel"))
                        {
                            EditorUNoteManager.DeleteNote(m_note);
                        }
                    }
                );    
            }
            
            menu.ShowAsContext();
        }
    }
}
