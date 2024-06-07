using System;
using System.Collections;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using TextAsset = UnityEngine.TextAsset;

namespace Violet.SheetManager.Editor
{
    [CustomEditor(typeof(Config))]
    public class SheetManagerSaveDataInspector : UnityEditor.Editor
    {
        public VisualTreeAsset asset;
        public VisualTreeAsset sheetItemAsset;
        private Config.Attribute _attribute;
        private SheetItem _selectedSheetItem = null;

        private bool _onProgress = false;

        protected enum Mode
        {
            Local,
            Remote,
        }

        protected Mode CurrentMode = Mode.Local;

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();
            asset.CloneTree(inspector);

            var component = serializedObject.targetObject as Config;
            _attribute = component.attribute;

            var versionFileField = inspector.Q<ObjectField>("version_file");
            versionFileField.value = component.versionFile;
            versionFileField.RegisterValueChangedCallback(evt =>
            {
                component.versionFile = (TextAsset)evt.newValue;
                EditorUtility.SetDirty(component);
                AssetDatabase.Refresh();
                RefreshVersionInfo(inspector);
            });

            //로컬 시트 경로
            var localSheetDirectoryField = inspector.Q<ObjectField>("sheet_local_directory");
            localSheetDirectoryField.value = component.localSheetDirectory;
            localSheetDirectoryField.RegisterValueChangedCallback(evt =>
            {
                component.localSheetDirectory = evt.newValue;
                EditorUtility.SetDirty(component);
                AssetDatabase.Refresh();
            });

            //로컬 시트 경로내의 xlsx파일 로드
            var sheetReloadButton = inspector.Q<Button>("btn_sheet_reload");
            sheetReloadButton.clickable.clicked += () =>
            {
                if (component.localSheetDirectory != null)
                {
                    _attribute.SheetItems.Clear();

                    var localSheetDirectoryPath = AssetDatabase.GetAssetPath(component.localSheetDirectory);
                    localSheetDirectoryPath = localSheetDirectoryPath.Replace("Assets/", "");
                    localSheetDirectoryPath = $"{Application.dataPath}/{localSheetDirectoryPath}";
                    var Files = Directory.GetFiles(localSheetDirectoryPath, "*.xlsx");
                    foreach (var File in Files)
                    {
                        var Path = File.Replace("\\", "/");
                        var excelAttribute = ExcelReader.Read(Path, component);
                        if (_attribute.Contains(excelAttribute.ClassName) == false)
                        {
                            var newSheetItem = new SheetItem();
                            newSheetItem.name = excelAttribute.ClassName;

                            var RelativePath = Path.Replace(Application.dataPath, "Assets");
                            newSheetItem.guid = AssetDatabase.GUIDFromAssetPath(RelativePath).ToString();
                            _attribute.SheetItems.Add(newSheetItem);
                        }
                    }

                    RefreshSheetList(inspector);
                    EditorUtility.SetDirty(component);
                    AssetDatabase.Refresh();
                }
            };

            var sheetSettingButton = inspector.Q<Button>("btn_excel_setting");
            sheetSettingButton.clickable.clicked += () =>
            {
                var excelSettingPopup = ExcelSettingWindow.Open();
                excelSettingPopup.LinkInformation(component.excelSettingInformation);
            };

            var sheetAddButton = inspector.Q<Button>("sheet_add_button");
            sheetAddButton.clickable.clicked += () =>
            {
                var newItem = new SheetItem();
                _attribute.SheetItems.Add(newItem);
                RefreshSheetList(inspector);
                EditorUtility.SetDirty(component);
                AssetDatabase.Refresh();
            };

            var baseUrlField = inspector.Q<TextField>("base_url");
            baseUrlField.value = _attribute.baseUrl;
            baseUrlField.RegisterValueChangedCallback(evt =>
            {
                _attribute.baseUrl = evt.newValue;
                EditorUtility.SetDirty(serializedObject.targetObject);
                AssetDatabase.Refresh();
            });

            var sheetList = inspector.Q<ListView>("sheet_list");
            sheetList.selectionChanged += obj =>
            {
                var itorator = obj.GetEnumerator();
                while (itorator.MoveNext())
                {
                    if (itorator.Current is SheetItem sheetItem)
                        _selectedSheetItem = sheetItem;
                }
            };

            var removeButton = inspector.Q<Button>("sheet_remove_button");
            removeButton.clickable.clicked += () =>
            {
                if (_selectedSheetItem != null)
                {
                    var message = $"Are you sure you want to remove [{_selectedSheetItem.name}]";
                    //Dialog 출력 후 OK 클릭시 아래 로직 실행
                    if (EditorUtility.DisplayDialog("Confirmation", message, "OK", "Cancel"))
                    {
                        _attribute.SheetItems.Remove(_selectedSheetItem);
                        _selectedSheetItem = null;
                        RefreshSheetList(inspector);
                        EditorUtility.SetDirty(component);
                        AssetDatabase.Refresh();
                    }
                }
            };

            var classGenerateDirectoryField = inspector.Q<ObjectField>("class_generate_directory");
            classGenerateDirectoryField.value = component.classGenerateDirectory;
            classGenerateDirectoryField.RegisterValueChangedCallback(evt =>
            {
                component.classGenerateDirectory = evt.newValue;
                EditorUtility.SetDirty(component);
                AssetDatabase.Refresh();
            });

            if (component.versionFile != null)
                RefreshVersionInfo(inspector);

            RefreshMode(inspector);
            RefreshSheetList(inspector);

            var btnGenerate = inspector.Q<Button>("generate_all");
            btnGenerate.clickable.clicked += () =>
            {
                foreach (var Item in component.attribute.SheetItems)
                {
                    var RelativeAssetPath = AssetDatabase.GUIDToAssetPath(new GUID(Item.guid));
                    var FullPath = RelativeAssetPath.Replace("Assets/", $"{Application.dataPath}/");
                    var ExcelAttribute = ExcelReader.Read(FullPath, component);
                    ExcelAttribute.ClassName = Item.name;
                    var GeneratePath = AssetDatabase.GetAssetPath(classGenerateDirectoryField.value);
                    Generator.ExcelToClass(ExcelAttribute, component, GeneratePath);
                }
            };

            // downloadButton.clickable.clicked += () =>
            // {
            //     EditorCoroutineUtility.StartCoroutineOwnerless(_GenerateProcess(inspector));
            // };


            return inspector;
        }

        private void RefreshVersionInfo(VisualElement visualElement)
        {
            var versionValueField = visualElement.Q<Label>("version_info");
            versionValueField.text = $"Version : {_attribute.version}";
        }

        private void RefreshMode(VisualElement visualElement)
        {
        }

        private void RefreshSheetList(VisualElement visualElement)
        {
            var sheetListView = visualElement.Q<ListView>("sheet_list");
            sheetListView.makeItem = () => sheetItemAsset.Instantiate();
            if (_attribute != null)
            {
                sheetListView.itemsSource = _attribute.SheetItems;
                sheetListView.bindItem = (element, i) =>
                {
                    var toggleButton = element.Q<Button>("btn_toggle");
                    toggleButton.clickable.clicked += () =>
                    {
                        _attribute.SheetItems[i].toggle = !_attribute.SheetItems[i].toggle;
                        OnRefreshToggleButton(element, i);
                        OnRefreshDetailGroup(element, i);
                    };

                    var sheetNameField = element.Q<TextField>("name");
                    sheetNameField.value = _attribute.SheetItems[i].name;
                    sheetNameField.RegisterValueChangedCallback(evt =>
                    {
                        _attribute.SheetItems[i].name = evt.newValue;
                        EditorUtility.SetDirty(serializedObject.targetObject);
                        AssetDatabase.Refresh();
                    });

                    var urlField = element.Q<TextField>("url");
                    urlField.value = _attribute.SheetItems[i].url;
                    urlField.RegisterValueChangedCallback(evt =>
                    {
                        _attribute.SheetItems[i].url = evt.newValue;
                        EditorUtility.SetDirty(serializedObject.targetObject);
                        AssetDatabase.Refresh();
                    });

                    var excelFileField = element.Q<ObjectField>("excel_file_field");
                    var assetPath = AssetDatabase.GUIDToAssetPath(new GUID(_attribute.SheetItems[i].guid));
                    excelFileField.value = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                    var title = element.Q<Label>("sheet_name_title");
                    title.text = assetPath.Split('/')[^1];

                    OnRefreshToggleButton(element, i);
                    OnRefreshDetailGroup(element, i);
                };
            }

            sheetListView.RefreshItems();
        }

        private bool IsGeneratedClass(string sheetName)
        {
            var component = serializedObject.targetObject as Config;

            if (component != null && component.classGenerateDirectory != null)
            {
                var path = AssetDatabase.GetAssetPath(component.classGenerateDirectory);
                if (AssetDatabase.IsValidFolder(path))
                {
                    string[] searchFolders = new[] { path };
                    if (AssetDatabase.FindAssets($"t:Script {sheetName}", searchFolders).Length > 0)
                        return true;
                }
            }

            return false;
        }

        private void OnRefreshToggleButton(VisualElement visualElement, int index)
        {
            var detailToggle = visualElement.Q<Button>("btn_toggle");
            detailToggle.text = _attribute.SheetItems[index].toggle ? "▼" : "▶";
        }

        private void OnRefreshDetailGroup(VisualElement visualElement, int index)
        {
            var detailGroup = visualElement.Q<VisualElement>("Information");
            detailGroup.style.display = _attribute.SheetItems[index].toggle ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnUpdateProgress(VisualElement visualElement, bool enable, string message, int value, int maxCount)
        {
            var progressBar = visualElement.Q<ProgressBar>("progress");
            if (enable == false)
            {
                progressBar.style.display = DisplayStyle.None;
                return;
            }

            progressBar.style.display = DisplayStyle.Flex;
            progressBar.title = message;
            progressBar.highValue = maxCount;
            progressBar.value = value;
        }

        private IEnumerator _GenerateProcess(VisualElement visualElement)
        {
            var component = serializedObject.targetObject as Config;
            int index = 0;
            foreach (var sheetItem in _attribute.SheetItems)
            {
                bool done = false;
                OnUpdateProgress(visualElement, true, sheetItem.name, ++index, _attribute.SheetItems.Count);
                EditorCoroutineUtility.StartCoroutineOwnerless(Downloader.Get($"{_attribute.baseUrl}{sheetItem.url}",
                    (result, text) =>
                    {
                        //Generate Class
                        if (result)
                        {
                            var assetPath = AssetDatabase.GetAssetPath(component.classGenerateDirectory);
                            Generator.CsvToClass(assetPath, sheetItem.name, text);
                        }

                        done = true;
                    }));

                while (done == false)
                    yield return null;

                RefreshSheetList(visualElement);
                AssetDatabase.Refresh();
            }
        }
    }
}
