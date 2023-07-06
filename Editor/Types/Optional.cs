using System;
using BluLib.Internal;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public class OptionalFloat : Optional<float>
	{
		public static OptionalFloat WithValue(float value)
		{
			return new() {IsSet = true, Value = value};
		}
	}

	[Serializable]
	public class OptionalInt : Optional<int>
	{
		public static OptionalInt WithValue(int value)
		{
			return new() {IsSet = true, Value = value};
		}
	}

	[Serializable]
	public class OptionalString : Optional<string>
	{
		public static OptionalString WithValue(string value)
		{
			return new() {IsSet = true, Value = value};
		}
	}

	[Serializable]
	public class OptionalKeyCode : Optional<KeyCode>
	{
		public static OptionalKeyCode WithValue(KeyCode value)
		{
			return new() {IsSet = true, Value = value};
		}
	}

	[Serializable]
	public class OptionalGameObject : Optional<GameObject>
	{
	}

	[Serializable]
	public class OptionalComponent : Optional<Component>
	{
	}
}

namespace BluLib.Internal
{
	[Serializable]
	public class Optional<T> : OptionalParent
	{
		public bool IsSet;
		public T Value;
	}

	[Serializable]
	public class OptionalParent
	{
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(OptionalParent), true)]
	public class OptionalTypePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			EditorGUI.indentLevel = 0; // PropertyDrawer Indent fix for nested inspectors

			SerializedProperty value = property.FindPropertyRelative("Value");
			SerializedProperty isSet = property.FindPropertyRelative("IsSet");

			int checkWidth = 14;
			int spaceWidth = 4;
			float valWidth = position.width - checkWidth - spaceWidth;

			position.width = checkWidth;
			isSet.boolValue = EditorGUI.Toggle(position, GUIContent.none, isSet.boolValue);

			position.x += checkWidth + spaceWidth;
			position.width = valWidth;
			if (isSet.boolValue) EditorGUI.PropertyField(position, value, GUIContent.none);
			EditorGUI.EndProperty();
		}
	}
}
#endif