using UnityEngine;
using UnityEditor;
using Umph.Core;

namespace Umph.Editor
{
    [CustomPropertyDrawer(typeof(UmphComponentEffect.BasicEffectSettings))]
    public class UmphBasicEffectSettingsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var delayProperty = property.FindPropertyRelative("Delay");
            var isParallelProperty = property.FindPropertyRelative("IsParallel");

            var leftRect = Rect.MinMaxRect(position.xMin, position.yMin, position.xMax - position.width * 0.6f, position.yMax);
            var rightRect = Rect.MinMaxRect(position.xMin + position.width * 0.6f, position.yMin, position.xMax - 50f, position.yMax);

            EditorGUI.BeginChangeCheck();
            isParallelProperty.boolValue = EditorGUI.ToggleLeft(leftRect, "Parallel", isParallelProperty.boolValue);


            var labelRect = Rect.MinMaxRect(rightRect.xMin, rightRect.yMin, rightRect.xMax - rightRect.width * 0.6f, rightRect.yMax);
            var floatRect = Rect.MinMaxRect(rightRect.xMin + rightRect.width * 0.6f, rightRect.yMin, rightRect.xMax, rightRect.yMax);

            EditorGUI.HandlePrefixLabel(rightRect, labelRect, new GUIContent("Delay"));
            delayProperty.floatValue = EditorGUI.FloatField(floatRect, delayProperty.floatValue);
            
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
