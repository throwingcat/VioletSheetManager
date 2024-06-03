using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Violet.SheetManager.Editor
{
    public static class Generator
    {
        public static void ExcelToClass(ExcelReader.ExcelAttribute InExcelAttribute, Config InConfig,string InGeneratePath)
        {
            string className = $"{InExcelAttribute.ClassName}";
            var code = "using System;\n" +
                       "using Violet.Sheet;\n" +
                       "namespace Violet.Sheet\n{\n" +
                       $"\tpublic partial class {className} : SheetDataBase\n" +
                       "\t{\n";

            for (int i = 0; i < InExcelAttribute.VariableNames.Count; i++)
            {
                string VariableType = InExcelAttribute.VariableTypes[i];
                
                //Excel의 Variable Type을 사용 가능한 변수형태로 변경
                if (InConfig.excelSettingInformation.ConvertType(VariableType, out VariableType))
                {
                    string VariableName = InExcelAttribute.VariableNames[i];
                    string line = $"public {VariableType} {VariableName} {{ get; set; }}\n";
                    code += $"\t\t{line}";    
                }
            }
            
            code += "\t}\n}";
            
            var classFilePath = $"{InGeneratePath}/{InExcelAttribute.ClassName}.cs";
            classFilePath = classFilePath.Replace("Assets/", $"{Application.dataPath}/");

            if (File.Exists(classFilePath))
                File.Delete(classFilePath);
            var stream = File.Create(classFilePath);
            var sw = new StreamWriter(stream);
            sw.Write(code);
            sw.Close();
        }

        public static void CsvToClass(string path, string fileName, string csv)
        {
            var lines = csv.Split('\n');
            var columnNames = lines[0].Split(',').ToArray();

            var dataRows = new string[lines.Length - 2][];
            for (var i = 0; i < lines.Length - 2; i++)
                dataRows[i] = lines[i + 1].Split(',').ToArray();

            var className = fileName;

            var code = "using System;\n" +
                       "using Violet.Sheet;\n" +
                       "namespace Violet.Sheet\n{\n" +
                       $"\tpublic partial class {className} : SheetDataBase\n" +
                       "\t{\n";
            for (var columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
            {
                var columnName = Regex.Replace(columnNames[columnIndex], @"[\s\.]", string.Empty,
                    RegexOptions.IgnoreCase);
                if (string.IsNullOrEmpty(columnName))
                    columnName = "Column" + (columnIndex + 1);

                //한글 주석 확인
                var byteCount = Encoding.Default.GetByteCount(columnName);
                if (byteCount != columnName.Length) continue;

                //Key 컬럼 제외
                if (columnName.Equals("key") || columnName.Equals("Key"))
                    continue;

                var values = new List<string>();
                for (int i = 0; i < dataRows.Length; i++)
                    values.Add(dataRows[i][columnIndex]);
                code += "\t\t" + GetVariableDeclaration(values.ToArray(), columnName) + "\n\n";
            }

            code += "\t}\n}";

            var classFilePath = $"{path}/{fileName}.cs";
            classFilePath = classFilePath.Replace("Assets/", $"{Application.dataPath}/");

            if (File.Exists(classFilePath))
                File.Delete(classFilePath);
            var stream = File.Create(classFilePath);
            var sw = new StreamWriter(stream);
            sw.Write(code);
            sw.Close();
        }

        private static string GetVariableDeclaration(string[] data, string columnName)
        {
            string typeAsString;
            if (AllIntValues(data))
                typeAsString = "int";
            else if (AllDoubleValues(data))
                typeAsString = "double";
            else if (AllBoolValues(data))
                typeAsString = "bool";
            else if (AllDateTimeValues(data))
                typeAsString = "DateTime";
            else
                typeAsString = "string";

            var declaration = $"public {typeAsString} {columnName} {{ get; set; }}";
            return declaration;
        }

        private static bool AllDoubleValues(string[] values)
        {
            double d;
            return values.All(val => double.TryParse(val, out d));
        }

        private static bool AllIntValues(string[] values)
        {
            int d;
            return values.All(val => int.TryParse(val, out d));
        }

        private static bool AllDateTimeValues(string[] values)
        {
            DateTime d;
            return values.All(val => DateTime.TryParse(val, out d));
        }

        private static bool AllBoolValues(string[] values)
        {
            bool d;
            return values.All(val => bool.TryParse(val, out d));
        }
    }
}