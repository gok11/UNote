using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNote.Editor
{
    public static class Utility
    {
        public static bool Initialized
        {
            get
            {
                return PathUtil.Initialized;
            }
        }
    }
}
