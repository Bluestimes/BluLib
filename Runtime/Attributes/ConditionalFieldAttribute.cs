using System;
using System.Linq;
using BluLib.Internal;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	/// <summary>
	/// Conditionally Show/Hide field in inspector, based on some other field or property value
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ConditionalFieldAttribute : PropertyAttribute
	{
		public bool IsSet => Data is { IsSet: true };
		public readonly ConditionalData Data;

		/// <param name="fieldToCheck">String name of field to check value</param>
		/// <param name="inverse">Inverse check result</param>
		/// <param name="compareValues">On which values field will be shown in inspector</param>
		public ConditionalFieldAttribute(string fieldToCheck, bool inverse = false, params object[] compareValues)
			=> Data = new(fieldToCheck, inverse, compareValues);

		
		public ConditionalFieldAttribute(string[] fieldToCheck, bool[] inverse = null, params object[] compare)
			=> Data = new(fieldToCheck, inverse, compare);

		public ConditionalFieldAttribute(params string[] fieldToCheck) => Data = new(fieldToCheck);
		public ConditionalFieldAttribute(bool useMethod, string method, bool inverse = false) 
			=> Data = new(useMethod, method, inverse);
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
	public class ConditionalFieldAttributeDrawer : PropertyDrawer
	{
		private bool toShow = true;
		private bool initialized;
		private PropertyDrawer customPropertyDrawer;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!(attribute is ConditionalFieldAttribute conditional)) return EditorGUI.GetPropertyHeight(property);
			
			CachePropertyDrawer(property);
			toShow = ConditionalUtility.IsPropertyConditionMatch(property, conditional.Data);
			if (!toShow) return -2;

			if (customPropertyDrawer != null) return customPropertyDrawer.GetPropertyHeight(property, label);
			return EditorGUI.GetPropertyHeight(property);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!toShow) return;

			if (!CustomDrawerUsed()) EditorGUI.PropertyField(position, property, label, true);

			
			bool CustomDrawerUsed()
			{
				if (customPropertyDrawer == null) return false;
				
				try
				{
					customPropertyDrawer.OnGUI(position, property, label);
					return true;
				}
				catch (Exception e)
				{
					WarningsPool.LogWarning(property,
						"Unable to use CustomDrawer of type " + customPropertyDrawer.GetType() + ": " + e,
						property.serializedObject.targetObject);

					return false;
				}
			}
		}
		
		/// <summary>
		/// Try to find and cache any PropertyDrawer or PropertyAttribute on the field
		/// </summary>
		private void CachePropertyDrawer(SerializedProperty property)
		{
			if (initialized) return;
			initialized = true;
			if (fieldInfo == null) return;

			PropertyDrawer customDrawer = CustomDrawerUtility.GetPropertyDrawerForProperty(property, fieldInfo, attribute);
			if (customDrawer == null) customDrawer = TryCreateAttributeDrawer();

			customPropertyDrawer = customDrawer;
			
			
			// Try to get drawer for any other Attribute on the field
			PropertyDrawer TryCreateAttributeDrawer()
			{
				Attribute secondAttribute = TryGetSecondAttribute();
				if (secondAttribute == null) return null;
				
				Type attributeType = secondAttribute.GetType();
				Type customDrawerType = CustomDrawerUtility.GetPropertyDrawerTypeForFieldType(attributeType);
				if (customDrawerType == null) return null;

				return CustomDrawerUtility.InstantiatePropertyDrawer(customDrawerType, fieldInfo, secondAttribute);
				
				
				//Get second attribute if any
				Attribute TryGetSecondAttribute()
				{
					return (PropertyAttribute)fieldInfo.GetCustomAttributes(typeof(PropertyAttribute), false)
						.FirstOrDefault(a => !(a is ConditionalFieldAttribute));
				}
			}
		}
	}
}
#endif
