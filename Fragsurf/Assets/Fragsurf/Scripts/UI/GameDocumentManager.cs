using UnityEngine;
using Fragsurf.Utility;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Linq;

namespace Fragsurf.UI
{
    public class GameDocumentManager : SingletonComponent<GameDocumentManager>
    {

        private class GameDocumentData
        {
            public string DocumentName;
            public Dictionary<string, string> ElementJson = new Dictionary<string, string>();
        }

        private List<GameDocument> _documents = new List<GameDocument>();
        private List<GameDocumentData> _documentData = new List<GameDocumentData>();

        private void Start()
        {
            DevConsole.RegisterObject(this);

            LoadDocumentData();
        }

        private void OnApplicationQuit()
        {
            SaveDocumentData();

            DevConsole.RemoveAll(this);
        }

        public void AddDocument(GameDocument doc)
        {
            _documents.Add(doc);

            PopulateDocumentData(doc);
        }

        public void RemoveDocument(GameDocument doc)
        {
            _documents.Remove(doc);
        }

        [ConCommand("doc.toggle", "Toggle a document's open state", ConVarFlags.Silent)]
        public void ToggleDocument(string name)
        {
            var doc = FindDocument(name);
            if (doc)
            {
                doc.IsOpen = !doc.IsOpen;
            }
        }

        [ConCommand("doc.open", "Opens a document", ConVarFlags.Silent)]
        public void OpenDocument(string name)
        {
            var doc = FindDocument(name);
            if (doc)
            {
                doc.IsOpen = true;
            }
        }

        [ConCommand("doc.close", "Closes a document", ConVarFlags.Silent)]
        public void CloseDocument(string name)
        {
            var doc = FindDocument(name);
            if (doc)
            {
                doc.IsOpen = false;
            }
        }

        public GameDocument FindDocument(string name)
        {
            foreach (var doc in _documents)
            {
                if (string.Equals(doc.DocumentName, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return doc;
                }
            }
            return null;
        }

        public bool HasCursor()
        {
            foreach (var doc in _documents)
            {
                if (doc.ShowCursor)
                {
                    return true;
                }
            }
            return false;
        }

        public bool InputHasFocus()
        {
            foreach(var doc in _documents)
            {
                if (doc.FocusedElement is TextField)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasFocus()
        {
            foreach (var doc in _documents)
            {
                if (doc.Focused)
                {
                    return true;
                }
            }
            return false;
        }

        public void Alert(string message)
        {
            Debug.Log("ALERT: " + message);
        }

        private void PopulateDocumentData(GameDocument doc)
        {
            var docData = _documentData.FirstOrDefault(x => string.Equals(x.DocumentName, doc.DocumentName));

            if (docData != null)
            {
                foreach (var kvp in docData.ElementJson)
                {
                    doc.UiDocument
                        .rootVisualElement
                        .Q<GameVisualElement>(kvp.Key)
                        ?.FromJson(kvp.Value);
                }
            }
        }

        private void LoadDocumentData()
        {
            try
            {
                var documentDataPath = Application.persistentDataPath + "/DocumentData.json";
                if (File.Exists(documentDataPath))
                {
                    var docDataJson = File.ReadAllText(documentDataPath);
                    DeserializeDocumentData(docDataJson);

                    foreach (var doc in _documents)
                    {
                        PopulateDocumentData(doc);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void SaveDocumentData()
        {
            var documentDataPath = Application.persistentDataPath + "/DocumentData.json";
            File.WriteAllText(documentDataPath, SerializeDocumentData());
        }

        private string SerializeDocumentData()
        {
            var docJsons = new List<GameDocumentData>();
            foreach (var doc in _documents)
            {
                var docObj = new GameDocumentData()
                {
                    DocumentName = doc.DocumentName
                };
                var elementJsons = new List<string>();
                doc.UiDocument.rootVisualElement.Query<GameVisualElement>().ForEach(e =>
                {
                    if (e is IPreserveData)
                    {
                        docObj.ElementJson.Add(e.name, e.ToJson());
                        elementJsons.Add(e.ToJson());
                    }
                });
                docJsons.Add(docObj);
            }
            return JsonConvert.SerializeObject(docJsons);
        }

        private void DeserializeDocumentData(string json)
        {
            _documentData.Clear();

            var arr = JArray.Parse(json);
            foreach (var el in arr)
            {
                try
                {
                    var docObj = JsonConvert.DeserializeObject<GameDocumentData>(el.ToString());
                    _documentData.Add(docObj);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

    }
}

