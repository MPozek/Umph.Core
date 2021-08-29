using UnityEngine;
using UnityEditor;
using Umph.Core;

namespace Umph.Editor
{
    [CustomPropertyDrawer(typeof(UmphComponentEffect))]
    public class UmphComponentEffectDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}
