using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Violet.SheetManager
{
    [System.Serializable]
    public class SheetItem
    {
        public string name;
        public string url;
    
        #if UNITY_EDITOR
        public bool showDetail = true;
        #endif
    }
}