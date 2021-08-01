using UnityEngine;

namespace Umph.Core
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionalFieldName;
        public object[] ConditionalValue;

        public ShowIfAttribute(string conditionalFieldName, params object[] conditionalValue)
        {
            ConditionalFieldName = conditionalFieldName;
            ConditionalValue = conditionalValue;
        }
    }
}
