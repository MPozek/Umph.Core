using UnityEngine;

using UnityEditor;

using Umph.Core;

namespace Umph.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attribute = (ShowIfAttribute) base.attribute;

            if (!string.IsNullOrEmpty(attribute.ConditionalFieldName))
            {
                // var field = property.serializedObject.FindProperty(attribute.ConditionalFieldName);
                var lastDotIndex = property.propertyPath.LastIndexOf('.');
                
                if (lastDotIndex >= 0)
                {
                    var path = property.propertyPath.Substring(0, lastDotIndex) + "." + attribute.ConditionalFieldName;
                    var field = property.serializedObject.FindProperty(path);

                    if (field != null && !GetValueAsObject(field, attribute.ConditionalValue))
                    {
                        return 0f;
                    }
                }
            }

            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attribute = (ShowIfAttribute)base.attribute;

            if (!string.IsNullOrEmpty(attribute.ConditionalFieldName))
            {
                // var field = property.serializedObject.FindProperty(attribute.ConditionalFieldName);
                var lastDotIndex = property.propertyPath.LastIndexOf('.');

                if (lastDotIndex >= 0)
                {
                    var path = property.propertyPath.Substring(0, lastDotIndex) + "." + attribute.ConditionalFieldName;
                    var field = property.serializedObject.FindProperty(path);

                    if (field != null && !GetValueAsObject(field, attribute.ConditionalValue))
                    {
                        return;
                    }
                }
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        private static bool GetValueAsObject(SerializedProperty property, object[] valuesToCompare)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                    return CompareValue(property.intValue, valuesToCompare);
            }

            return false;
        }

        private static bool CompareValue<T>(T fieldValue, object[] compareAgainst)
        {
            for (int i = 0; i < compareAgainst.Length; i++)
            {
                if (fieldValue.Equals((T)compareAgainst[i]))
                    return true;
            }
            return false;
        }
    }
}
