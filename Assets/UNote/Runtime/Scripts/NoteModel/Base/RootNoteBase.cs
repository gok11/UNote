using UnityEngine;

namespace UNote.Runtime
{
    public abstract class RootNoteBase : NoteBase
    {
        [SerializeField] private string m_noteName;
        
        public override NoteType NoteType => NoteType.Project;
        
        public override string NoteName => m_noteName;
        
        public void ChangeNoteName(string newName)
        {
            m_noteName = newName;
        }
    }
}
