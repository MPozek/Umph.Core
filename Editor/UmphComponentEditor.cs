using UnityEngine;

using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System;

using Umph.Core;

namespace Umph.Editor
{
    [CustomEditor(typeof(UmphComponent))]
    public class UmphComponentEditor : UnityEditor.Editor
    {
        private UmphComponent _target;
        private GenericMenu _typeSelectMenu;
        private SerializedProperty _effectListProperty;
        private ReorderableList _listDrawer;

        private void OnEnable()
        {
            _target = (UmphComponent) target;

            _effectListProperty = serializedObject.FindProperty("_effects");
            
            _listDrawer = new ReorderableList(serializedObject, _effectListProperty);

            InitializeAddDropdown();

            _listDrawer.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Sequence"); };

            _listDrawer.elementHeightCallback = GetListElementHeight;

            _listDrawer.drawElementBackgroundCallback += DrawListElementBackground;

            _listDrawer.drawElementCallback = DrawListElement;

            _listDrawer.displayAdd = true;
            _listDrawer.onAddDropdownCallback = DisplayAddDropdown;
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();

                if (((UmphComponent)target).IsPlaying)
                {
                    if (GUILayout.Button("Skip"))
                    {
                        ((UmphComponent)target).Skip();
                    }
                }
                else
                {
                    if (GUILayout.Button("Play"))
                    {
                        ((UmphComponent)target).Play();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            serializedObject.Update();

            var playOnStart = serializedObject.FindProperty("_playOnStart");
            EditorGUILayout.PropertyField(playOnStart);

            _listDrawer.DoLayoutList();

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

        private void InitializeAddDropdown()
        {
            _effectListProperty.InsertArrayElementAtIndex(0);
            var subtypeCache = new SubtypeCache(_effectListProperty.GetArrayElementAtIndex(0).managedReferenceFieldTypename);
            _effectListProperty.DeleteArrayElementAtIndex(0);

            _typeSelectMenu = new GenericMenu();

            for (int i = 0; i < subtypeCache.Count; i++)
            {
                var (name, type) = subtypeCache.GetSubtypeAt(i);

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

                _typeSelectMenu.AddItem(new GUIContent(displayName), false, AddEffect, type);
            }
        }

        private void DisplayAddDropdown(Rect buttonRect, ReorderableList list)
        {
            _typeSelectMenu.DropDown(buttonRect);
        }

        private void AddEffect(System.Object type)
        {
            var index = _effectListProperty.arraySize;
            _effectListProperty.InsertArrayElementAtIndex(index);
            
            var instance = (UmphComponentEffect) Activator.CreateInstance((Type)type);
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
            if (Application.isPlaying)
            {
                var seq = _target.Sequence;
                if (index == seq.CurrentEffectIndex && seq.IsPlaying)
                {
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.35f, 0.2f));
                    return;
                }
            }

            if (isActive || isFocused)
            {
                EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.35f));
            }
            else if (index % 2 == 1)
            {
                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
            }
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var property = _effectListProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(rect, property, true);
        }

        public class SubtypeCache
        {
            public class SubtypesInfo
            {
                public Type[] subtypes;
                public string[] displayNames;
            }

            private readonly string _baseTypeName;
            private readonly Type _baseType;
            private readonly SubtypesInfo _subtypes;

            public int Count => _subtypes.displayNames.Length;

            public (string, Type) GetSubtypeAt(int index)
            {
                return (_subtypes.displayNames[index], _subtypes.subtypes[index]);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="baseTypeName">The property type name as written in SerializedProperty.managedReferenceFieldTypename</param>
            public SubtypeCache(string baseTypeName)
            {
                _baseTypeName = baseTypeName;

                var words = baseTypeName.Split(' ');
                _baseType = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name == words.First()).SingleOrDefault().GetType(words.Last());

                _subtypes = new SubtypesInfo();
                _subtypes.subtypes = TypeCache.GetTypesDerivedFrom(_baseType).Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t) == false && t.IsAbstract == false).ToArray();
                _subtypes.displayNames = _subtypes.subtypes.Select(t => t.FullName.Replace("Umph.", "").Replace('.', '/')).ToArray();
            }
        }
    }
}
