using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Violet.Sheet;

namespace Violet.SheetManager
{
    public class SheetManager : MonoBehaviour
    {
        private static SheetManager _instance;

        public static SheetManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<SheetManager>();
                return _instance;
            }
        }

        [SerializeField] private Config _config;

        private Dictionary<string, Dictionary<string, SheetDataBase>> _sheets = new();

        public void Load(Action onComplete)
        {
            StartCoroutine(_Load(onComplete));
        }

        private IEnumerator _Load(Action onComplete)
        {
            foreach (var item in _config.attribute.SheetItems)
            {
                if (_sheets.ContainsKey(item.name) == false)
                    _sheets.Add(item.name, new());

                bool done = false;
                StartCoroutine(Downloader.Get($"{_config.attribute.baseUrl}{item.url}", (result, text) =>
                {
                    if (result)
                    {
                        string key = "";
                        Type type = Type.GetType("Violet.Sheet." + item.name);
                        var data = CSVReader.Read(new TextAsset(text));
                        foreach (var row in data)
                        {
                            var instance = Activator.CreateInstance(type);
                            foreach (var element in row)
                            {
                                string column = element.Key;
                                object value = element.Value;
                                var pi = type.GetProperty(column);
                                if (pi != null)
                                {
                                    pi.SetValue(instance, Convert.ChangeType(value, pi.PropertyType));
                                    if (column.Equals("key") || column.Equals("Key"))
                                        key = value.ToString();
                                    continue;
                                }

                                var fieldInfo = type.GetField(column);
                                if (fieldInfo != null)
                                {
                                    fieldInfo.SetValue(instance, Convert.ChangeType(value, fieldInfo.FieldType));
                                    if (column.Equals("key") || column.Equals("Key"))
                                        key = value.ToString();
                                }

                                _sheets[item.name][key] = instance as SheetDataBase;
                                var mi = type.GetMethod("Initialize");
                                mi.Invoke(_sheets[item.name][key], null);
                            }
                        }
                    }

                    done = true;
                }));

                while (done == false)
                    yield return null;
            }

            onComplete?.Invoke();
        }

        public Dictionary<string, SheetDataBase> Get<T>() where T : SheetDataBase
        {
            var key = typeof(T).Name;
            if (_sheets.TryGetValue(key, out var sheet))
                return sheet;
            return new Dictionary<string, SheetDataBase>();
        }

        public T Get<T>(string key) where T : SheetDataBase
        {
            var sheetName = typeof(T).Name;
            if (_sheets.TryGetValue(sheetName, out var sheet))
            {
                if (sheet.TryGetValue(key, out var data))
                    return data as T;
            }

            return default;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Violet/Sheet/Create Manager")]
        public static void Create()
        {
            var component = FindObjectOfType<SheetManager>();
            if (component != null) return;

            var root = GameObject.Find("[Violet]");
            if (root == null)
                root = new GameObject("[Violet]");
            var gameObject = new GameObject("Sheet Manager");
            gameObject.transform.SetParent(root.transform);
            component = gameObject.AddComponent<SheetManager>();
        }
#endif
    }
}