using UnityEditor;

namespace UNote.Editor
{
    public static class UxmlPath
    {
        public static readonly string InspectorNoteEditor = PathUtil.GetUxmlPath("InspectorNoteEditor.uxml");
        public static readonly string NoteContent = PathUtil.GetUxmlPath("NoteContent.uxml");
        public static readonly string NoteEditorCenterPane = PathUtil.GetUxmlPath("NoteEditorCenterPane.uxml");
        public static readonly string NoteEditorLeftPane = PathUtil.GetUxmlPath("NoteEditorLeftPane.uxml");
        public static readonly string NoteEditorRightPane = PathUtil.GetUxmlPath("NoteEditorRightPane.uxml");
        public static readonly string NoteInputField = PathUtil.GetUxmlPath("NoteInputField.uxml");
        public static readonly string NoteListItem = PathUtil.GetUxmlPath("NoteListItem.uxml");
        public static readonly string NoteTypeItem = PathUtil.GetUxmlPath("NoteTypeItem.uxml");
    }

    public static class UssPath
    {
        public static readonly string UNoteEditor = PathUtil.GetUssPath("UNoteEditor.uss");
    }
}
