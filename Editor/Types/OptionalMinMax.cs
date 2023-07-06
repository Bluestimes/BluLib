using System;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	/// <summary>
	/// Type to set Min/Max values but with optional Min and Max
	/// </summary>
	[Serializable]
	public struct OptionalMinMax
	{
		public bool MinIsSet;
		public bool MaxIsSet;
		
		public float Min;
		public float Max;

		public float GetFixed(float value)
		{
			if (MinIsSet && value < Min) value = Min;
			if (MaxIsSet && value > Max) value = Max;

			return value;
		}

		public OptionalMinMax(bool minIsSet, bool maxIsSet, float min, float max)
		{
			MinIsSet = minIsSet;
			MaxIsSet = maxIsSet;
			Min = min;
			Max = max;
		}
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof (OptionalMinMax))]
	public class MinMaxFloatPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			EditorGUI.indentLevel = 0; // PropertyDrawer Indent fix for nested inspectors
			
			SerializedProperty minProp = property.FindPropertyRelative("Min");
			SerializedProperty maxProp = property.FindPropertyRelative("Max");
			SerializedProperty minCheckProp = property.FindPropertyRelative("MinIsSet");
			SerializedProperty maxCheckProp = property.FindPropertyRelative("MaxIsSet");

			string minMaxLabel = "Min : Max";
			int labelWidth = 58;
			int checkWidth = 14;
			int spaceWidth = 4;
			float valWidth = (position.width / 2) - (labelWidth / 2f) - (spaceWidth * 4) - checkWidth + 2;
			
			position.width = valWidth;
			if (minCheckProp.boolValue) EditorGUI.PropertyField(position, minProp, GUIContent.none);
			
			position.x += valWidth + spaceWidth;
			position.width = checkWidth;
			minCheckProp.boolValue = EditorGUI.Toggle(position, GUIContent.none, minCheckProp.boolValue);
			
			position.x += checkWidth + spaceWidth;
			position.width = labelWidth;
			EditorGUI.LabelField(position, minMaxLabel);
			
			position.x += labelWidth + spaceWidth;
			position.width = checkWidth;
			maxCheckProp.boolValue = EditorGUI.Toggle(position, GUIContent.none, maxCheckProp.boolValue);
			
			position.x += checkWidth + spaceWidth;
			position.width = valWidth;
			if (maxCheckProp.boolValue) EditorGUI.PropertyField(position, maxProp, GUIContent.none);

			if (GUI.changed)
			{
				if (maxCheckProp.boolValue && maxCheckProp.boolValue)
				{
					if (maxProp.floatValue < minProp.floatValue) maxProp.floatValue = minProp.floatValue;
				}
				property.serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.EndProperty();
		}
	}
}
#endif