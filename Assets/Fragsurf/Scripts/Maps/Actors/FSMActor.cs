//using RealtimeCSG;
//using RealtimeCSG.Components;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fragsurf.Actors
{
    [SelectionBase]
    [System.Serializable]
    public class FSMActor : MonoBehaviour
    {

        [Header("Actor Options")]
        [FormerlySerializedAs("_targetName")]
        public string ActorName;
        //[HideInInspector]
        [SerializeField]
        [FormerlySerializedAs("_fields")]
        protected ActorField[] _customProperties;

        public virtual void Tick() { }
        protected virtual void _Update() { }
        protected virtual void _Awake() { }
        protected virtual void _Start() { }
        protected virtual void _OnDestroy() { }
        protected virtual void _OnDrawGizmos() { }

        private void Awake()
        {
            // backwards compatibility
            CustomPropertiesToFields();

            _Awake();
        }

        private void Start()
        {
            if (ActorName == "Ladder")
            {
                gameObject.tag = "Ladder";
            }
            EnableRenderers(false);
            _Start();
        }

        private void Update()
        {
            _Update();
        }

        private void OnDrawGizmos()
        {
            var center = transform.position;
            //var brush = GetComponentInChildren<CSGBrush>();
            //if(brush)
            //{
            //    center = BoundsUtilities.GetCenter(brush);
            //}
            DebugDraw.WorldLabel($"{GetType().Name}\n<color=#00f742>{ActorName}</color>", center, 12, Color.yellow, 30f);
            _OnDrawGizmos();
        }

        public void EnableRenderers(bool enabled)
        {
            foreach(var renderer in GetComponentsInChildren<Renderer>())
            {
                if(renderer is LineRenderer)
                {
                    continue;
                }
                renderer.enabled = enabled;
            }
        }

        public virtual void Refresh() { }

        public ActorField GetCustomProperty(string name)
        {
            for (int i = 0; i < _customProperties.Length; i++)
            {
                if (_customProperties[i].Name == name)
                {
                    return _customProperties[i];
                }
            }
            return null;
        }

        // This is to support really old maps
        private void CustomPropertiesToFields()
        {
            var thisType = GetType();

            if (_customProperties != null)
            {
                foreach (var field in _customProperties)
                {
                    var actualField = thisType.GetField(field.Name.Replace(" ", null));
                    if (actualField != null)
                    {
                        switch (field.FieldType)
                        {
                            case ActorFieldType.Bool:
                                actualField.SetValue(this, field.BoolValue);
                                break;
                            case ActorFieldType.Float:
                                actualField.SetValue(this, field.FloatValue);
                                break;
                            case ActorFieldType.Int:
                                actualField.SetValue(this, field.IntValue);
                                break;
                            case ActorFieldType.String:
                                actualField.SetValue(this, field.StringValue);
                                break;
                            case ActorFieldType.TriggerCondition:
                                actualField.SetValue(this, field.TriggerConditionValue);
                                break;
                            case ActorFieldType.Vector2:
                                actualField.SetValue(this, field.Vector2Value);
                                break;
                            case ActorFieldType.Vector3:
                                actualField.SetValue(this, field.Vector3Value);
                                break;
                        }
                    }
                }
            }
        }

    }
}

