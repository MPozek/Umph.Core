using UnityEngine;

using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

using Umph.Core;

namespace Umph.Editor
{
    [CustomPropertyDrawer(typeof(UmphComponentEffect))]
    public class UmphComponentEffectDrawer : PropertyDrawer
    {
        private const float PARALLEL_INDENT = 20f;

        private static readonly HashSet<string> nonAlignedProperties = new HashSet<string>() { "IsParallel" };
        private static readonly HashSet<string> predrawnProperties = new HashSet<string>() { "Delay", "IsParallel" };

        public static System.Type GetManagedType(SerializedProperty property)
        {
            var words = property.managedReferenceFieldTypename.Split(' ');
            var t = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name == words.First()).SingleOrDefault().GetType(words.Last());

            return t;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var t = GetManagedType(property);
            if (t == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var propertyBackup = property.Copy();
            var count = propertyBackup.CountInProperty();
            property = property.Copy();

            float h = EditorGUIUtility.singleLineHeight;

            if (count > 1)
            {
                property.Next(true);

                for (int i = 0; i < count - 1; i++)
                {
                    if (nonAlignedProperties.Contains(property.name) == false)
                    {
                        var ph = EditorGUI.GetPropertyHeight(property);

                        if (ph > 0f)
                        {
                            h += ph + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }

                    if (i + 2 != count)
                    {
                        property.Next(false);
                    }
                }
            }

            return h;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                EditorGUI.LabelField(rect, "Can't resolve the type name");
                return;
            }

            rect.xMin += 10f;

            var propertyBackup = property.Copy();
            var isParallelProperty = property.FindPropertyRelative("IsParallel");

            var count = property.CountInProperty();
            property = propertyBackup;

            if (isParallelProperty.boolValue)
            {
                rect.xMin += PARALLEL_INDENT;
            }
            else
            {
                rect.xMax -= PARALLEL_INDENT;
            }

            var name = property.managedReferenceFullTypename.Split('.');

            DrawComponentLabel(ref rect, property, name[name.Length - 1], isParallelProperty);

            if (property.isExpanded)
            {
                if (count > 1)
                {
                    EditorGUI.indentLevel++;

                    DrawBaseProperties(ref rect, property);

                    DrawOtherProperties(rect, count, property);

                    EditorGUI.indentLevel--;
                }
            }            
        }

        private void DrawComponentLabel(ref Rect rect, SerializedProperty property, string name, SerializedProperty isParallelProperty)
        {
            var h = EditorGUI.GetPropertyHeight(property, false);
            var r = rect;
            r.yMax = r.yMin + h;
            rect.yMin += r.height;


            var substringIndex = name.IndexOf("ComponentEffect");
            if (substringIndex < 0)
            {
                substringIndex = name.Length;
            }

            var displayName = "";
            for (int j = 0; j < substringIndex; j++)
            {
                var c = name[j];
                if (j > 0 && char.IsUpper(c))
                {
                    displayName += " ";
                }

                displayName += c;
            }

            var labelRect = r;
            labelRect.xMax -= 25f;
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, displayName);

            var parallelRect = r;
            parallelRect.xMin = parallelRect.xMax - 25f;

            var content = new GUIContent("P", "Parallel");
            isParallelProperty.boolValue = EditorGUI.ToggleLeft(parallelRect, content, isParallelProperty.boolValue);
        }

        private void DrawBaseProperties(ref Rect rect, SerializedProperty property)
        {
            var delayProperty = property.FindPropertyRelative("Delay");

            var h = EditorGUI.GetPropertyHeight(delayProperty);
            var r = rect;
            r.yMax = r.yMin + h;

            EditorGUI.PropertyField(r, delayProperty);
            delayProperty.floatValue = Mathf.Max(0f, delayProperty.floatValue);

            rect.yMin += h + EditorGUIUtility.standardVerticalSpacing;
        }

        private void DrawOtherProperties(Rect rect, int propertyCount, SerializedProperty property)
        {
            // draw other properties
            property.Next(true);
            for (int i = 0; i < propertyCount - 1; i++)
            {
                if (predrawnProperties.Contains(property.name) == false)
                {
                    EditorGUI.PropertyField(rect, property, true);
                    var ph = EditorGUI.GetPropertyHeight(property);

                    if (ph > 0f)
                    {
                        var h = ph + EditorGUIUtility.standardVerticalSpacing;
                        rect.yMin += h;
                    }
                }

                if (i + 2 != propertyCount)
                {
                    property.Next(false);
                }
            }
        }
    }
}
