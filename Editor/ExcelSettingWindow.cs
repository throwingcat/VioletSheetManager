using UnityEditor;
using UnityEngine.UIElements;

namespace Violet.SheetManager.Editor
{
    public class ExcelSettingWindow : EditorWindow
    {
        public static ExcelSettingWindow Instance;

        public static ExcelSettingWindow Open()
        {
            if (Instance == null)
            {
                Instance = CreateInstance<ExcelSettingWindow>();
                Instance.Show();
            }
            else
                Instance.Focus();

            return Instance;
        }


        public VisualTreeAsset asset;
        public VisualTreeAsset typeCastingItemAsset;
        private Config.ExcelSettingInformation _excelSettingInformation;
        private System.Action<Config.ExcelSettingInformation> _onChangeEvent;

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            
            var inspector = new VisualElement();
            asset.CloneTree(inspector);
            rootVisualElement.Add(inspector);
        }

        public void LinkInformation(Config.ExcelSettingInformation InExcelSettingInformation)
        {
            _excelSettingInformation = InExcelSettingInformation;

            var VariableTypeLine = rootVisualElement.Q<IntegerField>("variable_type_line");
            VariableTypeLine.value = _excelSettingInformation.VariableTypeLine;
            VariableTypeLine.RegisterValueChangedCallback(evt =>
            {
                _excelSettingInformation.VariableTypeLine = evt.newValue;
                _onChangeEvent?.Invoke(_excelSettingInformation);
            });

            var VariableNameLine = rootVisualElement.Q<IntegerField>("variable_name_line");
            VariableNameLine.value = _excelSettingInformation.VariableNameLine;
            VariableNameLine.RegisterValueChangedCallback(evt =>
            {
                _excelSettingInformation.VariableNameLine = evt.newValue;
                _onChangeEvent?.Invoke(_excelSettingInformation);
            });

            var ValueStartLine = rootVisualElement.Q<IntegerField>("value_start_line");
            ValueStartLine.value = _excelSettingInformation.ValueStartLine;
            ValueStartLine.RegisterValueChangedCallback(evt =>
            {
                _excelSettingInformation.ValueStartLine = evt.newValue;
                _onChangeEvent?.Invoke(_excelSettingInformation);
            });

            var typeCastingAddButton = rootVisualElement.Q<Button>("btn_type_casting_add");
            typeCastingAddButton.clickable.clicked += () =>
            {
                _excelSettingInformation.TypeInformations.Add(new());
                RefreshTypeCastingList(rootVisualElement);
            };
            RefreshTypeCastingList(rootVisualElement);
        }

        private void RefreshTypeCastingList(VisualElement visualElement)
        {
            var list = visualElement.Q<ListView>("type_casting_list_view");
            list.makeItem = () => typeCastingItemAsset.Instantiate();

            if (_excelSettingInformation != null)
            {
                list.itemsSource = _excelSettingInformation.TypeInformations;
                list.bindItem = (element, i) =>
                {
                    var attribute = _excelSettingInformation.TypeInformations[i];
                    var deleteButton = element.Q<Button>("btn_delete");
                    deleteButton.clickable.clicked += () =>
                    {
                        _excelSettingInformation.TypeInformations.RemoveAt(i);
                        CreateGUI();
                        LinkInformation(_excelSettingInformation);
                    };

                    var fromField = element.Q<TextField>("from_type");
                    fromField.value = attribute.From;
                    fromField.RegisterValueChangedCallback((v) =>
                    {
                        attribute.From = v.newValue;
                        //RefreshTypeCastingList(visualElement);
                    });

                    var toField = element.Q<TextField>("to_type");
                    toField.value = attribute.To;
                    toField.RegisterValueChangedCallback((v) =>
                    {
                        attribute.To = v.newValue;
                        //RefreshTypeCastingList(visualElement);
                    });
                };
            }

            list.RefreshItems();
        }
    }
}