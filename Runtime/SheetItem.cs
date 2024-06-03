using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Violet.SheetManager
{
    [System.Serializable]
    public class SheetItem
    {
        public string name;
        public string url;

        #if UNITY_EDITOR
        public GUID guid;
        public bool toggle = false;
        #endif
    }
}