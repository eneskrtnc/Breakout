using UnityEngine;

namespace SpaceTrader.Data
{
    public abstract class BaseDef : ScriptableObject
    {
        public string id;
        public string displayName;

        [TextArea]
        public string desc;
        public string[] tags;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = name.Replace(" ", "_").ToLowerInvariant();
        }
#endif
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
    public sealed class DataLabelAttribute : System.Attribute
    {
        public readonly string Label;

        public DataLabelAttribute(string label) => Label = label;
    }
}
