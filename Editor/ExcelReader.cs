using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using ExcelDataReader;
using UnityEngine;
using Violet.SheetManager;

public static class ExcelReader
{
    public class ExcelAttribute
    {
        public string FileName;
        public string ClassName;
        public List<string> VariableTypes = new();
        public List<string> VariableNames = new();
        public List<string[]> Values = new();
    }

    public static ExcelAttribute Read(string InPath, Config InConfig)
    {
        ExcelAttribute attribute = new();
        if (File.Exists(InPath))
        {
            using (var Stream = File.Open(InPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var Reader = ExcelReaderFactory.CreateReader(Stream))
                {
                    //파일 이름
                    attribute.FileName = InPath.Split('/')[^1];
                    attribute.FileName = attribute.FileName.Replace(".xlsx", "");

                    //클래스 이름
                    int LetterStartIndex = 0;
                    for (int i = 0; i < attribute.FileName.Length; i++)
                    {
                        if (char.IsLetter(attribute.FileName[i]))
                        {
                            LetterStartIndex = i;
                            break;
                        }
                    }

                    attribute.ClassName = attribute.FileName.Substring(LetterStartIndex);

                    int ValueIndex = 0;
                    for (int row = 0; row < Reader.RowCount; row++)
                    {
                        Reader.Read();
                        for (int field = 0; field < Reader.FieldCount; field++)
                        {
                            var Value = Reader.GetValue(field);
                            if (Value == null)
                                Debug.LogError(
                                    $"[Exception] [{attribute.FileName} / Row:{row},Field:{field}] Field is Empty");

                            //필드 변수 타입
                            if (row == InConfig.excelSettingInformation.VariableTypeLine - 1)
                            {
                                if (Value != null)
                                    attribute.VariableTypes.Add(Value.ToString());
                            }
                            //필드 변수 이름
                            else if (row == InConfig.excelSettingInformation.VariableNameLine - 1)
                            {
                                if (Value != null)
                                {
                                    var FieldName = Value.ToString();
                                    FieldName = FieldName.Replace(" ", "");
                                    attribute.VariableNames.Add(FieldName);
                                }
                            }
                            //필드 변수 값
                            else if (row >= InConfig.excelSettingInformation.ValueStartLine - 1)
                            {
                                while (attribute.Values.Count <= ValueIndex)
                                    attribute.Values.Add(new string[attribute.VariableNames.Count]);
                                if (Value != null && field < attribute.Values[ValueIndex].Length)
                                {
                                    attribute.Values[ValueIndex][field] = Value.ToString();
                                }
                            }
                        }

                        if (row >= InConfig.excelSettingInformation.ValueStartLine - 1)
                            ValueIndex++;
                    }

                    Reader.Close();
                }

                Stream.Close();
            }
        }

        return attribute;
    }
}