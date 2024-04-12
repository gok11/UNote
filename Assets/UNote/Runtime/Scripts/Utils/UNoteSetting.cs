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
    }
}
