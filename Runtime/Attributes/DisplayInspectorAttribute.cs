using System.Collections.Generic;
using System.Reflection;
using BluLib.EditorTools;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	/// <summary>
	/// Use to display inspector of property object
	/// </summary>
	public class DisplayInspectorAttribute : PropertyAttribute
	{
		public readonly bool DisplayScript;

		public DisplayInspectorAttribute(bool displayScriptField = true)
		{
			DisplayScript = displayScriptField;
		}
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(DisplayInspectorAttribute))]
	public class DisplayInspectorAttributeDrawer : PropertyDrawer
	{
		private ButtonMethodHandler buttonMethods;
		private EditorPrefsBool foldout;

		private readonly Dictionary<Object, SerializedObject> targets = new();

		private SerializedObject GetTargetSo(Object targetObject)
		{
			SerializedObject target;
			if (targets.TryGetValue(targetObject, out SerializedObject target1)) target = target1;
			else
			{
				targets.Add(targetObject, new(targetObject));
				target = targets[targetObject];
			}

			target.Update();
			return target;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			bool notValidType = property.propertyType != SerializedPropertyType.ObjectReference;
			if (notValidType)
			{
				EditorGUI.LabelField(position, label.text, "Use [DisplayInspector] with MB or SO");
				return;
			}
			
			position.height = EditorGUIUtility.singleLineHeight;
			bool displayScript = ((DisplayInspectorAttribute)attribute).DisplayScript;
			if (displayScript || property.objectReferenceValue == null)
			{
				// Draw foldout only if Script line drawn and there is content to hide (ref assigned)
				if (property.objectReferenceValue != null)
				{
					// Workaround to make label clickable, accurately aligned and property field click is not triggering foldout
					Rect foldRect = new(position);
					foldRect.width = EditorGUIUtility.labelWidth;
					foldout.Value = EditorGUI.Foldout(foldRect, foldout.Value, new GUIContent(""), true, StyleFramework.FoldoutHeader);
					EditorGUI.PropertyField(position, property, label);
					if (GUI.changed) property.serializedObject.ApplyModifiedProperties();
					
					if (!foldout.Value) return;
				}
				else
				{
					EditorGUI.PropertyField(position, property, label);
					if (GUI.changed) property.serializedObject.ApplyModifiedProperties();
				}
			}
			if (property.objectReferenceValue == null) return;

			if (buttonMethods == null) buttonMethods = new(property.objectReferenceValue);

			if (displayScript) position.y += position.height + 4;
			float startY = position.y - 2;
			float startX = position.x;

			SerializedObject target = GetTargetSo(property.objectReferenceValue);
			SerializedProperty propertyObject = target.GetIterator();
			propertyObject.Next(true);
			propertyObject.NextVisible(true);

			float xPos = position.x + 10;
			float width = position.width - 10;

			bool expandedReorderable = false;
			while (propertyObject.NextVisible(propertyObject.isExpanded && !expandedReorderable))
			{
#if UNITY_2020_2_OR_NEWER
				expandedReorderable = propertyObject.isExpanded && propertyObject.isArray &&
				                      !propertyObject.IsAttributeDefined<NonReorderableAttribute>();
#endif
				position.x = xPos + 10 * propertyObject.depth;
				position.width = width - 10 * propertyObject.depth;

				position.height = EditorGUI.GetPropertyHeight(propertyObject, expandedReorderable);
				EditorGUI.PropertyField(position, propertyObject, expandedReorderable);

				position.y += position.height + 4;
			}

			if (!buttonMethods.TargetMethods.IsNullOrEmpty())
			{
				foreach ((MethodInfo Method, string Name, ButtonMethodDrawOrder Order, ConditionalData Condition) method in buttonMethods.TargetMethods)
				{
					position.height = EditorGUIUtility.singleLineHeight;
					if (GUI.Button(position, method.Name)) buttonMethods.Invoke(method.Method);
					position.y += position.height;
				}
			}

			Rect bgRect = position;
			bgRect.y = startY;
			bgRect.x = startX - 12;
			bgRect.width = 11;
			bgRect.height = position.y - startY;
			if (buttonMethods.Amount > 0) bgRect.height += 5;

			DrawColouredRect(bgRect, new(.6f, .6f, .8f, .5f));


			target.ApplyModifiedProperties();
			property.serializedObject.ApplyModifiedProperties();
		}

		private bool IsFolded(SerializedProperty property)
		{
			if(foldout == null)
			{
				foldout = new("DisplayInspectorFoldout" +
                              property.GetParent().GetType().Name +
                              property.propertyPath);
			}
			return foldout.Value;
		}
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			bool notValidType = property.propertyType != SerializedPropertyType.ObjectReference;
			bool displayScript = ((DisplayInspectorAttribute)attribute).DisplayScript;
			if (notValidType || property.objectReferenceValue == null || (displayScript && !IsFolded(property))) return base.GetPropertyHeight(property, label);
			
			if (buttonMethods == null) buttonMethods = new(property.objectReferenceValue);
			float height = displayScript ? EditorGUI.GetPropertyHeight(property) + 4 : 0;

			SerializedObject target = GetTargetSo(property.objectReferenceValue);
			SerializedProperty propertyObject = target.GetIterator();
			propertyObject.Next(true);
			propertyObject.NextVisible(true);

			bool expandedReorderable = false;
			while (propertyObject.NextVisible(propertyObject.isExpanded && !expandedReorderable))
			{
#if UNITY_2020_2_OR_NEWER
				expandedReorderable = propertyObject.isExpanded && propertyObject.isArray &&
				                      !propertyObject.IsAttributeDefined<NonReorderableAttribute>();
#endif
				height += EditorGUI.GetPropertyHeight(propertyObject, expandedReorderable) + 4;
			}

			if (buttonMethods.Amount > 0) height += 4 + buttonMethods.Amount * EditorGUIUtility.singleLineHeight;
			return height;
		}

		private void DrawColouredRect(Rect rect, Color color)
		{
			Color defaultBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = color;
			GUI.Box(rect, "");
			GUI.backgroundColor = defaultBackgroundColor;
		}
	}
}
#endif
