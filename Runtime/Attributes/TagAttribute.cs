// ---------------------------------------------------------------------------- 
// Author: Kaynn, Yeo Wen Qin
// https://github.com/Kaynn-Cahya
// Date:   11/02/2019
// ----------------------------------------------------------------------------

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BluLib
{
	public class TagAttribute : PropertyAttribute
	{
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
	[CustomPropertyDrawer(typeof(TagAttribute))]
	public class TagAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				if (!@checked) Warning(property);
				EditorGUI.PropertyField(position, property, label);
				return;
			}

			property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
		}

		private bool @checked;

		private void Warning(SerializedProperty property)
		{
			Debug.LogWarning(
                $"Property <color=brown>{property.name}</color> in object <color=brown>{property.serializedObject.targetObject}</color> is of wrong type. Expected: String");
			@checked = true;
		}
	}
}
#endif
