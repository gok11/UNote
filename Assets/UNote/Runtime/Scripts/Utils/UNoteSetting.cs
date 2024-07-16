using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UNote.Runtime
{
    public class UNoteSetting : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private string m_userName;

        public string UserName
        {
            get => m_userName;
#if UNITY_EDITOR
            set
            {
                if (m_userName != value)
                {
                    m_userName = value;
                }
            }
#endif
        }

#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        private bool m_inspectorFoldoutOpened;

        public bool InspectorFoldoutOpened
        {
            get => m_inspectorFoldoutOpened;
            set => m_inspectorFoldoutOpened = value;
        }
#endif // UNITY_EDITOR
    }
}
