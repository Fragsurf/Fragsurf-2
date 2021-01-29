using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fragsurf.UI
{
    public struct Percent
    {
        public static StyleLength New(float length)
        {
            return new StyleLength(new Length(length, LengthUnit.Percent));
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class GameVisualElement : VisualElement
    {
        public UIDocument Document { get; set; }

        [JsonProperty]
        private string _serializedElementName { get; set; }
        private string _documentJson;
        protected bool _initialized { get; private set; }

        public GameVisualElement() 
        {
            RegisterCallback<GeometryChangedEvent>(e =>
            {
                if (!_initialized)
                {
                    Init();
                    _initialized = true;

                    if (!string.IsNullOrEmpty(_documentJson))
                    {
                        FromJson(_documentJson);
                    }
                }
                GeometryChanged(e);
            });
        }

        private void Init()
        {
            Initialize();
        }

        public virtual void Update() { }
        protected virtual void Initialize() { }
        protected virtual void GeometryChanged(GeometryChangedEvent e) { }

        protected void LoadTemplate(string path)
        {
            var resource = Resources.Load<VisualTreeAsset>(path);
            if (resource)
            {
                Add(resource.CloneTree());
            }
            else
            {
                Debug.LogError("Missing template: " + path);
            }
        }

        public string ToJson()
        {
            _serializedElementName = name;
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public void FromJson(string json)
        {
            if (!_initialized)
            {
                _documentJson = json;
                return;
            }
            JsonConvert.PopulateObject(json, this);
        }

    }
}

