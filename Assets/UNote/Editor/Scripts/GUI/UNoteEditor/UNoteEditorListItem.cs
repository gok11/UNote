using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UNote.Runtime;

namespace UNote.Editor
{
    public class UNoteEditorListItem : VisualElement
    {
        private VisualElement m_parentContainer;

        private NoteBase m_note;

        private VisualElement m_archvieLabel;
        private VisualElement m_favoriteLabel;
        private Label m_nameLabel;
        private Label m_noteContentLabel;
        private Button m_contextButton;
        private VisualElement m_noteListItem;

        public NoteBase BindNote => m_note;

        public UNoteEditorListItem(UNoteEditor noteEditor)
        {
            VisualTreeAsset listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                UxmlPath.NoteListItem
            );
            TemplateContainer template = listItem.CloneTree();

            m_archvieLabel = template.Q<VisualElement>("ArchiveLabel");
            m_favoriteLabel = template.Q<VisualElement>("FavoriteLabel");
            m_nameLabel = template.Q<Label>("Name");
            m_noteContentLabel = template.Q<Label>("ContentLine");
            m_contextButton = template.Q<Button>("ContextButton");

            contentContainer.Add(template);

            m_noteListItem = contentContainer.Q("NoteListItem");

            m_contextButton.visible = false;

            m_archvieLabel.style.backgroundImage = StyleUtil.ArchiveIcon;
            m_favoriteLabel.style.backgroundImage = StyleUtil.FavoriteIcon;
            
            EditorUNoteManager.OnNoteArchvied += note =>
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

            m_archvieLabel.style.display = note.Archived ? DisplayStyle.Flex : DisplayStyle.None;

            switch (note.NoteType)
            {
                case NoteType.Project:
                {
                    ProjectNoteComment comment = EditorUNoteManager
                        .GetProjectLeafNoteListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.CreatedDate)
                        .FirstOrDefault();

                    m_noteContentLabel.text = comment
                        ?.NoteContent.Replace("\r", " ")
                        .Replace("\n", " ");

                    RegisterMouseEvent();
                    break;
                } 

                case NoteType.Asset:
                {
                    AssetNoteComment comment = EditorUNoteManager
                        .GetAssetLeafNoteListByNoteId(note.NoteId)
                        .OrderByDescending(t => t.CreatedDate)
                        .FirstOrDefault();

                    m_noteContentLabel.text = comment
                        ?.NoteContent.Replace("\r", " ")
                        .Replace("\n", " ");
                    
                    // TODO
                    RegisterMouseEvent();
                    break;
                }
            }
            Focus();
        }

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

        public void SetBackgroundColor(bool select)
        {
            m_noteListItem.style.backgroundColor = select ?
                StyleUtil.SelectColor : StyleUtil.UnselectColor;
        }

        private void ShowContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            bool isOwnNote = m_note.Author == UNoteSetting.UserName;

            if (m_note.NoteType == NoteType.Project && isOwnNote)
            {
                menu.AddItem(
                    new GUIContent("Rename"),
                    false,
                    () =>
                    {
                        EditorWindow.GetWindow<UNoteEditor>().RightPane.EnableChangeTitleMode();
                    }
                );
            }

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

        private void ShowArchiveLabel()
        {
            
        }
    }
}
