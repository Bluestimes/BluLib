// ---------------------------------------------------------------------------- 
// Author: Dimitry, PixeyeHQ
// Project : UNITY FOLDOUT
// https://github.com/PixeyeHQ/InspectorFoldoutGroup
// Contacts : Pix - ask@pixeye.games
// Website : http://www.pixeye.games
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BluLib
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public readonly string Name;
        public readonly bool FoldEverything;

        /// <summary>Adds the property to the specified foldout group.</summary>
        /// <param name="name">Name of the foldout group.</param>
        /// <param name="foldEverything">Toggle to put all properties to the specified group</param>
        public FoldoutAttribute(string name, bool foldEverything = false)
        {
            FoldEverything = foldEverything;
            Name = name;
        }
    }
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    public class FoldoutAttributeHandler
    {
        private readonly Dictionary<string, CacheFoldProp> cacheFoldouts = new();
        private readonly List<SerializedProperty> props = new();
        private bool initialized;

        private readonly Object target;
        private readonly SerializedObject serializedObject;
        
        public bool OverrideInspector => props.Count > 0;
        
        public FoldoutAttributeHandler(Object target, SerializedObject serializedObject)
        {
            this.target = target;
            this.serializedObject = serializedObject;
        }

        public void OnDisable()
        {
            if (target == null) return;

            foreach (KeyValuePair<string, CacheFoldProp> c in cacheFoldouts)
            {
                EditorPrefs.SetBool(string.Format($"{c.Value.Attribute.Name}{c.Value.Properties[0].name}{target.name}"), c.Value.Expanded);
                c.Value.Dispose();
            }
        }

        public void Update()
        {
            serializedObject.Update();
            Setup();
        }

        public void OnInspectorGUI()
        {
            Header();
            Body();

            serializedObject.ApplyModifiedProperties();
        }

        private void Header()
        {
            using (new EditorGUI.DisabledScope("m_Script" == props[0].propertyPath))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(props[0], true);
                EditorGUILayout.Space();
            }
        }

        private void Body()
        {
            foreach (KeyValuePair<string, CacheFoldProp> pair in cacheFoldouts)
            {
                EditorGUILayout.BeginVertical(StyleFramework.Box);
                Foldout(pair.Value);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.Space();

            for (int i = 1; i < props.Count; i++)
            {
                EditorGUILayout.PropertyField(props[i], true);
            }

            EditorGUILayout.Space();
        }

        private void Foldout(CacheFoldProp cache)
        {
            cache.Expanded = EditorGUILayout.Foldout(cache.Expanded, cache.Attribute.Name, true, StyleFramework.FoldoutHeader);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x -= 18;
            rect.y -= 4;
            rect.height += 8;
            rect.width += 22;
            EditorGUI.LabelField(rect, GUIContent.none, EditorStyles.helpBox);
            
            if (cache.Expanded)
            {
                EditorGUILayout.Space(2);
                
                foreach (SerializedProperty property in cache.Properties)
                {
                    EditorGUILayout.BeginVertical(StyleFramework.BoxChild);
                    EditorGUILayout.PropertyField(property, new(ObjectNames.NicifyVariableName(property.name)), true);
                    EditorGUILayout.EndVertical();
                }
            }
        }
        
        private void Setup()
        {
            if (initialized) return;

            FoldoutAttribute prevFold = default;

            int length = EditorTypes.Get(target, out List<FieldInfo> objectFields);

            for (int i = 0; i < length; i++)
            {
                #region FOLDERS

                FoldoutAttribute fold = Attribute.GetCustomAttribute(objectFields[i], typeof(FoldoutAttribute)) as FoldoutAttribute;
                CacheFoldProp c;
                if (fold == null)
                {
                    if (prevFold != null && prevFold.FoldEverything)
                    {
                        if (!cacheFoldouts.TryGetValue(prevFold.Name, out c))
                        {
                            cacheFoldouts.Add(prevFold.Name,
                                new() {Attribute = prevFold, Types = new() {objectFields[i].Name}});
                        }
                        else
                        {
                            c.Types.Add(objectFields[i].Name);
                        }
                    }

                    continue;
                }

                prevFold = fold;

                if (!cacheFoldouts.TryGetValue(fold.Name, out c))
                {
                    bool expanded = EditorPrefs.GetBool(string.Format($"{fold.Name}{objectFields[i].Name}{target.name}"), false);
                    cacheFoldouts.Add(fold.Name,
                        new() {Attribute = fold, Types = new() {objectFields[i].Name}, Expanded = expanded});
                }
                else c.Types.Add(objectFields[i].Name);

                #endregion
            }

            SerializedProperty property = serializedObject.GetIterator();
            bool next = property.NextVisible(true);
            if (next)
            {
                do
                {
                    HandleFoldProp(property);
                } while (property.NextVisible(false));
            }

            initialized = true;
        }

        private void HandleFoldProp(SerializedProperty prop)
        {
            bool shouldBeFolded = false;

            foreach (KeyValuePair<string, CacheFoldProp> pair in cacheFoldouts)
            {
                if (pair.Value.Types.Contains(prop.name))
                {
                    SerializedProperty pr = prop.Copy();
                    shouldBeFolded = true;
                    pair.Value.Properties.Add(pr);

                    break;
                }
            }

            if (shouldBeFolded == false)
            {
                SerializedProperty pr = prop.Copy();
                props.Add(pr);
            }
        }

        private class CacheFoldProp
        {
            public HashSet<string> Types = new();
            public readonly List<SerializedProperty> Properties = new();
            public FoldoutAttribute Attribute;
            public bool Expanded;

            public void Dispose()
            {
                Properties.Clear();
                Types.Clear();
                Attribute = null;
            }
        }
    }


    static class StyleFramework
    {
        public static readonly GUIStyle Box;
        public static readonly GUIStyle BoxChild;
        public static readonly GUIStyle FoldoutHeader;

        static StyleFramework()
        {
            FoldoutHeader = new(EditorStyles.foldout);
            FoldoutHeader.overflow = new(-10, 0, 3, 0);
            FoldoutHeader.padding = new(20, 0, 0, 0);
            FoldoutHeader.border = new(2, 2, 2, 2);

            Box = new(GUI.skin.box);
            Box.padding = new(18, 0, 4, 4);

            BoxChild = new(GUI.skin.box);
        }
    }

    static class EditorTypes
    {
        private static readonly Dictionary<int, List<FieldInfo>> fields = new(FastComparable.Default);

        public static int Get(System.Object target, out List<FieldInfo> objectFields)
        {
            Type t = target.GetType();
            int hash = t.GetHashCode();

            if (!fields.TryGetValue(hash, out objectFields))
            {
                IList<Type> typeTree = GetTypeTree(t);
                objectFields = target.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.NonPublic)
                    .OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
                    .ToList();
                fields.Add(hash, objectFields);
            }

            return objectFields.Count;
        }
        
        static IList<Type> GetTypeTree(Type t)
        {
            List<Type> types = new();
            while (t.BaseType != null)
            {
                types.Add(t);
                t = t.BaseType;
            }

            return types;
        }
    }


    internal class FastComparable : IEqualityComparer<int>
    {
        public static readonly FastComparable Default = new();

        public bool Equals(int x, int y)
        {
            return x == y;
        }

        public int GetHashCode(int obj)
        {
            return obj.GetHashCode();
        }
    }
}
#endif
