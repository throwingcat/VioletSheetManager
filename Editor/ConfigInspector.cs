using System.Collections;
using Unity.EditorCoroutines.Editor;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
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

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();
            var component = serializedObject.targetObject as Config;
            _attribute = component.attribute;
            asset.CloneTree(inspector);

            var versionFileField = inspector.Q<ObjectField>("version_file");
            versionFileField.value = component.versionFile;
            versionFileField.RegisterValueChangedCallback(evt =>
            {
                component.versionFile = (TextAsset)evt.newValue;
                EditorUtility.SetDirty(component);
                AssetDatabase.Refresh();
                RefreshVersionInfo(inspector);
            });

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

            RefreshSheetList(inspector);

            var downloadButton = inspector.Q<Button>("generate_all");
            downloadButton.clickable.clicked += () =>
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(_GenerateProcess(inspector));
            };


            return inspector;
        }

        private void RefreshVersionInfo(VisualElement visualElement)
        {
            var versionValueField = visualElement.Q<Label>("version_info");
            versionValueField.text = $"Version : {_attribute.version}";
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
                    var sheetNameField = element.Q<TextField>("name");
                    sheetNameField.value = _attribute.SheetItems[i].name;
                    sheetNameField.RegisterValueChangedCallback(evt =>
                    {
                        _attribute.SheetItems[i].name = evt.newValue;
                        EditorUtility.SetDirty(serializedObject.targetObject);
                        AssetDatabase.Refresh();
                    });


                    var checkMark = element.Q<VisualElement>("check_mark");
                    var questionMark = element.Q<VisualElement>("question_mark");
                    checkMark.style.display = IsGeneratedClass(_attribute.SheetItems[i].name)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                    questionMark.style.display = IsGeneratedClass(_attribute.SheetItems[i].name)
                        ? DisplayStyle.None
                        : DisplayStyle.Flex;

                    var detailToggle = element.Q<Button>("detail_toggle_button");
                    detailToggle.clickable.clicked += () =>
                    {
                        _attribute.SheetItems[i].showDetail = !_attribute.SheetItems[i].showDetail;
                        OnRefreshToggleButton(element, i);
                        OnRefreshDetailGroup(element, i);
                    };

                    var urlField = element.Q<TextField>("url");
                    urlField.value = _attribute.SheetItems[i].url;
                    urlField.RegisterValueChangedCallback(evt =>
                    {
                        _attribute.SheetItems[i].url = evt.newValue;
                        EditorUtility.SetDirty(serializedObject.targetObject);
                        AssetDatabase.Refresh();
                    });

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
            var detailToggle = visualElement.Q<Button>("detail_toggle_button");
            detailToggle.text = _attribute.SheetItems[index].showDetail ? "▼" : "◀";
        }

        private void OnRefreshDetailGroup(VisualElement visualElement, int index)
        {
            var detailGroup = visualElement.Q<VisualElement>("Detail");
            detailGroup.style.display = _attribute.SheetItems[index].showDetail ? DisplayStyle.Flex : DisplayStyle.None;
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
                EditorCoroutineUtility.StartCoroutineOwnerless(Downloader.Get($"{_attribute.baseUrl}{sheetItem.url}", (result,text) =>
                {
                    //Generate Class
                    if (result)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(component.classGenerateDirectory);
                        Generator.CsvToClass(assetPath,sheetItem.name,text);    
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