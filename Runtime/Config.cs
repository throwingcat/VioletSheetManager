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
        public Object localSheetDirectory;

        public Attribute attribute = new();
        public ExcelSettingInformation excelSettingInformation = new();
        
        [System.Serializable]
        public class Attribute
        {
            public string version;
            public string baseUrl = "";
            public List<SheetItem> SheetItems = new();

            public bool Contains(string InSheetName)
            {
                foreach (var sheetItem in SheetItems)
                {
                    if (sheetItem.name.Equals(InSheetName))
                        return true;
                }

                return false;
            }
        }

        [System.Serializable]
        public class ExcelSettingInformation
        {
            public int VariableTypeLine;
            public int VariableNameLine;
            public int ValueStartLine;

            public List<ExcelSettingVariableTypeInformation> TypeInformations = new();

            public bool ConvertType(string InType,out string Result)
            {
                Result = "";
                foreach (var Information in TypeInformations)
                {
                    if (Information.From.Equals(InType))
                    {
                        Result = Information.To;
                        return true;
                    }
                }
                return false;
            }
        }

        [System.Serializable]
        public class ExcelSettingVariableTypeInformation
        {
            public string From;
            public string To;
        }
    }
}