using UnityEditor;
using UnityEngine;

namespace BluLib
{
	public class LayerAttribute : PropertyAttribute
	{
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
	public class LayerAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.Integer)
			{
				if (!@checked) Warning(property);
				EditorGUI.PropertyField(position, property, label);
				return;
			}
			
			property.intValue = EditorGUI.LayerField(position, label, property.intValue);
		}

		private bool @checked;

		private void Warning(SerializedProperty property)
		{
			Debug.LogWarning(
                $"Property <color=brown>{property.name}</color> in object <color=brown>{property.serializedObject.targetObject}</color> is of wrong type. Expected: Int");
			@checked = true;
		}
	}
}
#endif
