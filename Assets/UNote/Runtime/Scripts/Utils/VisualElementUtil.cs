using UnityEngine.UIElements;

namespace UNote.Runtime
{
    public static class VisualElementUtil
    {
        public static void AddSpacer(this VisualElement ve)
        {
            VisualElement spacer = new VisualElement();
            spacer.name = "Spacer";
            spacer.style.flexGrow = 1;
            ve.Add(spacer);
        }
    }
}
