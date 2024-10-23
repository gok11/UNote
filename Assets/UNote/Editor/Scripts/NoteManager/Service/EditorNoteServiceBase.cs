using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    /// <summary>
    /// Editor note base service
    /// </summary>
    internal abstract class EditorNoteServiceBase
    {
        protected static string NoteAssetDirectory => Path.Combine("Assets", "UNote", "NoteAssets");
        
        /// <summary>
        /// Generate unique note name
        /// </summary>
        protected static string GetUniqueName<T>(string baseName, IReadOnlyList<T> noteList) where T : NoteBase
        {
            string noteName = baseName;
            int id = 0;
            
            while (true)
            {
                bool isOverlap = false;
                foreach (var note in noteList)
                {
                    if (note.NoteName == noteName)
                    {
                        isOverlap = true;
                        break;
                    }
                }

                if (!isOverlap)
                {
                    break;
                }

                noteName = $"{baseName} {id++}";
            }
            
            return noteName;
        }
    }
}
