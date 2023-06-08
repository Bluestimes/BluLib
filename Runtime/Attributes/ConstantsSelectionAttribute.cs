using System;
using System.Collections.Generic;
using System.Reflection;
using BluLib.EditorTools;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	public class ConstantsSelectionAttribute : PropertyAttribute
	{
		public readonly Type SelectFromType;

		public ConstantsSelectionAttribute(Type type)
		{
			SelectFromType = type;
		}
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(ConstantsSelectionAttribute))]
	public class ConstantsSelectionAttributeDrawer : PropertyDrawer
	{
		private ConstantsSelectionAttribute thisAttribute;
		private readonly List<MemberInfo> constants = new();
		private string[] names;
		private object[] values;
		private Type targetType;
		private int selectedValueIndex;
		private bool valueFound;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (thisAttribute == null) Initialize(property);
			if (values.IsNullOrEmpty() || selectedValueIndex < 0)
			{
				EditorGUI.PropertyField(position, property, label);
				return;
			}

			if (!valueFound && selectedValueIndex == 0) BluGUI.DrawColouredRect(position, BluGUI.Colors.Yellow);

			EditorGUI.BeginChangeCheck();
			selectedValueIndex = EditorGUI.Popup(position, label.text, selectedValueIndex, names);
			if (EditorGUI.EndChangeCheck())
			{
				fieldInfo.SetValue(property.serializedObject.targetObject, values[selectedValueIndex]);
				property.serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
		}

		private object GetValue(SerializedProperty property)
		{
			return fieldInfo.GetValue(property.serializedObject.targetObject);
		}
		
		private void Initialize(SerializedProperty property)
		{
			thisAttribute = (ConstantsSelectionAttribute) attribute;
			targetType = fieldInfo.FieldType;

			BindingFlags searchFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
			FieldInfo[] allPublicStaticFields = thisAttribute.SelectFromType.GetFields(searchFlags);
			PropertyInfo[] allPublicStaticProperties = thisAttribute.SelectFromType.GetProperties(searchFlags);

			// IsLiteral determines if its value is written at compile time and not changeable
			// IsInitOnly determines if the field can be set in the body of the constructor
			// for C# a field which is readonly keyword would have both true but a const field would have only IsLiteral equal to true
			foreach (FieldInfo field in allPublicStaticFields)
			{
				if ((field.IsInitOnly || field.IsLiteral) && field.FieldType == targetType)
					constants.Add(field);
			}
			foreach (PropertyInfo prop in allPublicStaticProperties)
			{
				if (prop.PropertyType == targetType) constants.Add(prop);
			}
			

			if (constants.IsNullOrEmpty()) return;
			names = new string[constants.Count];
			values = new object[constants.Count];
			for (int i = 0; i < constants.Count; i++)
			{
				names[i] = constants[i].Name;
				values[i] = GetValue(i);
			}

			object currentValue = GetValue(property);
			if (currentValue != null)
			{
				for (int i = 0; i < values.Length; i++)
				{
					if (currentValue.Equals(values[i]))
					{
						valueFound = true;
						selectedValueIndex = i;
					}
				}
			}
			
			if (!valueFound)
			{
				names = names.InsertAt(0);
				values = values.InsertAt(0);
				object actualValue = GetValue(property);
				object value = actualValue != null ? actualValue : "NULL";
				names[0] = "NOT FOUND: " + value;
				values[0] = actualValue;
			}
		}

		private object GetValue(int index)
		{
			MemberInfo member = constants[index];
			if (member.MemberType == MemberTypes.Field) return ((FieldInfo) member).GetValue(null);
			if (member.MemberType == MemberTypes.Property) return ((PropertyInfo) member).GetValue(null);
			return null;
		}
	}
}
#endif
