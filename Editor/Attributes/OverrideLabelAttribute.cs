using UnityEditor;
using UnityEngine;

namespace BluLib
{
	public class OverrideLabelAttribute : PropertyAttribute
	{
		public readonly string NewLabel;

		public OverrideLabelAttribute(string newLabel) => NewLabel = newLabel;
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(OverrideLabelAttribute))]
	public class OverrideLabelDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			PropertyDrawer customDrawer = CustomDrawerUtility.GetPropertyDrawerForProperty(property, fieldInfo, attribute);
			if (customDrawer != null) return customDrawer.GetPropertyHeight(property, label);
			
			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.isArray) WarningsPool.LogCollectionsNotSupportedWarning(property, nameof(OverrideLabelAttribute));
			
			label.text = ((OverrideLabelAttribute)attribute).NewLabel;

			PropertyDrawer customDrawer = CustomDrawerUtility.GetPropertyDrawerForProperty(property, fieldInfo, attribute);
			if (customDrawer != null) customDrawer.OnGUI(position, property, label);
			else EditorGUI.PropertyField(position, property, label, true);
		}
	}
}
#endif