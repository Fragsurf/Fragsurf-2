using UnityEngine;

namespace Fragsurf.Actors
{
    public enum ActorFieldType
    {
        None,
        String,
        Int,
        Float,
        Bool,
        Vector2,
        Vector3,
        TriggerCondition
    }

    [System.Serializable]
    public class ActorField
    {
        public string Name;
        public ActorFieldType FieldType;
        public string StringValue;
        public int IntValue;
        public float FloatValue;
        public bool BoolValue;
        public Vector2 Vector2Value;
        public Vector3 Vector3Value;
        public FSMTriggerCondition TriggerConditionValue;
        public bool BuiltIn = false;
    }
}
