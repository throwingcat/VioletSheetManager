using System.Collections.Generic;
using UnityEngine;

namespace Violet.SheetManager
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    #if UNITY_EDITOR
    [CreateAssetMenu(fileName = "Config",menuName = "Violet/SheetManager/Config")]
    #endif
    public class Config : ScriptableObject
    {
        public TextAsset versionFile;
        public Object classGenerateDirectory;

        public Attribute attribute = new();
        
        [System.Serializable]
        public class Attribute
        {
            public string version;
            public string baseUrl = "";
            public List<SheetItem> SheetItems = new();
        }
    }
}