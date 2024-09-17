using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNote.Runtime;

namespace UNote.Editor
{
    public class FavoriteNoteContainer : NoteContainerBase
    {
        #region Field

        [SerializeField] private List<string> m_favoriteNoteIdList = new ();

        #endregion // Field

        #region Internal Method

        internal List<string> GetFavoriteNoteList() => m_favoriteNoteIdList;

        #endregion // Internal Method
    }
}
