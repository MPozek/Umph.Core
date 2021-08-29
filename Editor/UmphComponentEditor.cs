using UnityEngine;

using UnityEditor;
using UnityEditorInternal;
using System;

using Umph.Core;
using UnityEditor.IMGUI.Controls;
using System.Linq;

namespace Umph.Editor
{

    [CustomEditor(typeof(UmphComponent))]
    public class UmphComponentEditor : UnityEditor.Editor
    {
        private Rect _listRect;

        private UmphComponent _target;
        private UmphComponentDropdown _typeSelectMenu;
        private SerializedProperty _effectListProperty;
        private ReorderableList _listDrawer;

        private UmphComponentMenu[] _componentDisplayData;

        private int _deleteElementIndex = -1;
        private SubtypeCache _subtypeCache;

        private void OnEnable()
        {
            _target = (UmphComponent) target;

            _effectListProperty = serializedObject.FindProperty("_effects");
            
            _listDrawer = new ReorderableList(serializedObject, _effectListProperty);

            InitializeSubtypeCache();

            InitializeAddDropdown();

            _listDrawer.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Sequence"); };

            _listDrawer.elementHeightCallback = GetListElementHeight;

            _listDrawer.drawElementBackgroundCallback += DrawListElementBackground;

            _listDrawer.drawElementCallback = DrawListElement;

            _listDrawer.displayAdd = true;
            _listDrawer.onAddDropdownCallback = (rect, list) => _typeSelectMenu.Show(_listRect);
            _listDrawer.displayRemove = false;
        }

        private void InitializeSubtypeCache()
        {
            _subtypeCache = new SubtypeCache(typeof(UmphComponentEffect));
            _componentDisplayData = new UmphComponentMenu[_subtypeCache.Count];
            for (int i = 0; i < _subtypeCache.Count; i++)
            {
                var t = _subtypeCache.GetSubtypeAt(i);
                var displayData = (UmphComponentMenu) t.GetCustomAttributes(typeof(UmphComponentMenu), false).FirstOrDefault();
                if (displayData == null)
                {
                    displayData = new UmphComponentMenu(t.Name.Replace("ComponentEffect", ""), t.FullName.Replace('.', '/').Replace("ComponentEffect", ""));
                }
                _componentDisplayData[i] = displayData;
            }
        }

        private void InitializeAddDropdown()
        {
            _typeSelectMenu = new UmphComponentDropdown(_componentDisplayData, new AdvancedDropdownState());
            _typeSelectMenu.OnItemSelected += AddEffect;
        }

        public override void OnInspectorGUI()
        {
            DrawPlayControls();

            if (_deleteElementIndex >= 0)
            {
                _effectListProperty.DeleteArrayElementAtIndex(_deleteElementIndex);
                _effectListProperty.serializedObject.ApplyModifiedProperties();
                _deleteElementIndex = -1;
            }

            serializedObject.Update();

            var playOnStart = serializedObject.FindProperty("_settings");
            EditorGUILayout.PropertyField(playOnStart);

            _listDrawer.DoLayoutList();

            if (Event.current.type == EventType.Repaint)
                _listRect = GUILayoutUtility.GetLastRect();

            if (serializedObject.ApplyModifiedProperties())
            {
                if (Application.isPlaying)
                {
                    if (_target.IsPlaying)
                    {
                        _target.Skip();
                    }

                    _target.Initialize(true);
                }
            }
        }

        private void DrawPlayControls()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();

                if (((UmphComponent)target).IsPlaying)
                {
                    if (GUILayout.Button("Pause"))
                    {
                        ((UmphComponent)target).Pause();
                    }

                    if (GUILayout.Button("Skip"))
                    {
                        ((UmphComponent)target).Skip();
                    }
                }
                else
                {
                    GUI.enabled = !((UmphComponent)target).IsCompleted;

                    if (GUILayout.Button("Play"))
                    {
                        ((UmphComponent)target).Play();
                    }

                    GUI.enabled = true;
                }

                if (GUILayout.Button("Reset"))
                {
                    ((UmphComponent)target).DoReset();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void AddEffect(int typeIndex)
        {
            var index = _effectListProperty.arraySize;
            _effectListProperty.InsertArrayElementAtIndex(index);

            var type = _subtypeCache.GetSubtypeAt(typeIndex);
            var instance = (UmphComponentEffect) Activator.CreateInstance(type);
            instance.EDITOR_Initialize(((Component)target).gameObject);

            _effectListProperty.GetArrayElementAtIndex(index).managedReferenceValue = instance;

            serializedObject.ApplyModifiedProperties();
        }

        private float GetListElementHeight(int index)
        {
            var elementProperty = _effectListProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(elementProperty);
        }

        private void DrawListElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _effectListProperty.arraySize)
                return;

            if (Application.isPlaying)
            {
                var seq = _target.Sequence;
                if (seq != null && index == seq.CurrentEffectIndex && seq.IsPlaying)
                {
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.35f, 0.2f));
                    return;
                }
            }

            if (index % 2 == 1)
            {
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
            }

            var isParallel = _effectListProperty.GetArrayElementAtIndex(index).FindPropertyRelative("Settings.IsParallel").boolValue;
            var hasPreviousElement = index > 0;
            var nextElementIsParallel =
                index + 1 < _effectListProperty.arraySize
                && _effectListProperty.GetArrayElementAtIndex(index + 1).FindPropertyRelative("Settings.IsParallel").boolValue;

            if ((isParallel && hasPreviousElement) || nextElementIsParallel)
            {
                EditorGUI.DrawRect(
                    Rect.MinMaxRect(
                        rect.xMin + 22f,
                        isParallel && hasPreviousElement ? rect.yMin : (rect.yMin + EditorGUIUtility.singleLineHeight * 0.5f), 
                        rect.xMin + 25f, 
                        nextElementIsParallel ? rect.yMax : (rect.yMin + EditorGUIUtility.singleLineHeight * 0.5f)
                    ), 
                    new Color32(58, 121, 197, 255) // unity blue
                );
            }
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var buttonRect = Rect.MinMaxRect(rect.xMax - 50f, rect.yMin, rect.xMax, rect.yMin + EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "Delete", EditorStyles.miniButton))
            {
                Event.current.Use();
                _deleteElementIndex = index;
                Repaint();
            }
            
            if (index < _effectListProperty.arraySize)
            {
                const float INDENT = 20f;
                var property = _effectListProperty.GetArrayElementAtIndex(index);
                rect.width -= INDENT;
                rect.center += Vector2.right * INDENT;

                var managedType = FindManagedTypeFromName(property.managedReferenceFullTypename);
                var typeIndex = _subtypeCache.GetTypeIndex(managedType);
                EditorGUI.PropertyField(rect, property, new GUIContent(_componentDisplayData[typeIndex].Name), true);
            }
        }

        private static Type FindManagedTypeFromName(string fullTypename)
        {
            var words = fullTypename.Split(' ');
            var t = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name == words.First()).SingleOrDefault().GetType(words.Last());
            return t;
        }
    }
}
