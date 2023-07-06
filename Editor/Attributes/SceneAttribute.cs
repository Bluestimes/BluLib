// ---------------------------------------------------------------------------- 
// Author: Anton
// https://github.com/antontidev
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	/// <summary>
	/// Used to pick scene from inspector.
	/// Consider to use <see cref="SceneReference"/> type instead as it is more flexible
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class SceneAttribute : PropertyAttribute
	{
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
	public class SceneDrawer : PropertyDrawer
	{
		private SceneAttribute thisAttribute;
		private string[] scenesInBuild;
		private int index;

		private void Initialize(string initialValue)
		{
			if (thisAttribute != null) return;

			thisAttribute = (SceneAttribute)attribute;
			
			scenesInBuild = new string[EditorBuildSettings.scenes.Length + 1];

			index = 0;
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				string formatted = EditorBuildSettings.scenes[i].path.Split('/').Last().Replace(".unity", string.Empty);
				if (initialValue == formatted) index = i + 1;
				formatted += $" [{i}]";
				scenesInBuild[i + 1] = formatted;
			}

			string defaultValue = "NULL";
			if (initialValue.NotNullOrEmpty() && index == 0) defaultValue = "NOT FOUND: " + initialValue;
			scenesInBuild[0] = defaultValue;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, label.text, "Use [Scene] with strings.");
				return;
			}
			
			Initialize(property.stringValue);
			
			int newIndex = EditorGUI.Popup(position, label.text, index, scenesInBuild);
			if (newIndex != index)
			{
				index = newIndex;
				string value = scenesInBuild[index];
				property.stringValue = newIndex == 0 ? string.Empty : value.Substring(0, value.IndexOf('[') - 1);
				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
#endif