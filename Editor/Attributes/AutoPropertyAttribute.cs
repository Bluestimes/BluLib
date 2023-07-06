using System;
using System.Collections.Generic;
using System.Linq;
using BluLib.EditorTools;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BluLib
{
	/// <summary>
	/// Automatically assign components to this Property.
	/// It searches for components from this GO or its children by default.
	/// Pass in an <c>AutoPropertyMode</c> to override this behaviour.
	/// <para></para>
	/// Advanced usage: Filter found objects with a method. To do that, create a 
	/// static method or member method of the current class with the same method
	/// signature as a Func&lt;UnityEngine.Object, bool&gt;. Your predicate method
	/// can be private.
	/// If your predicate method is a member method of the current class, pass in
	/// the nameof that method as the second argument.
	/// If your predicate method is a static method, pass in the typeof class that
	/// contains said method as the third argument.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class AutoPropertyAttribute : PropertyAttribute
	{
		public readonly AutoPropertyMode Mode;
		public readonly string PredicateMethodName;
		public readonly Type PredicateMethodTarget;

		public AutoPropertyAttribute(AutoPropertyMode mode = AutoPropertyMode.Children,
			string predicateMethodName = null,
			Type predicateMethodTarget = null)
		{
			Mode = mode;
			PredicateMethodTarget = predicateMethodTarget;
			PredicateMethodName = predicateMethodName;
		}
	}

	public enum AutoPropertyMode
	{
		/// <summary>
		/// Search for Components from this GO or its children.
		/// </summary>
		Children = 0,
		/// <summary>
		/// Search for Components from this GO or its parents.
		/// </summary>
		Parent = 1,
		/// <summary>
		/// Search for Components from this GO's current scene.
		/// </summary>
		Scene = 2,
		/// <summary>
		/// Search for Objects from this project's asset folder.
		/// </summary>
		Asset = 3,
		/// <summary>
		/// Search for Objects from anywhere in the project.
		/// Combines the results of Scene and Asset modes.
		/// </summary>
		Any = 4
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
#if UNITY_2021_2_OR_NEWER
	using UnityEditor.SceneManagement;
#else
	using UnityEditor.Experimental.SceneManagement;
#endif

    [CustomPropertyDrawer(typeof(AutoPropertyAttribute))]
	public class AutoPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = true;
		}
	}


	[InitializeOnLoad]
	public static class AutoPropertyHandler
	{
		private static readonly Dictionary<AutoPropertyMode, Func<BluEditor.ObjectField, Func<Object, bool>, Object[]>> objectsGetters
			= new()
            {
				[AutoPropertyMode.Children] = (property, pred) => property.Context
					.As<Component>()
					?.GetComponentsInChildren(property.Field.FieldType.GetElementType() ?? property.Field.FieldType, true)
					.Where(pred).ToArray(),
				[AutoPropertyMode.Parent] = (property, pred) => property.Context.As<Component>()
					?.GetComponentsInParent(property.Field.FieldType.GetElementType() ?? property.Field.FieldType, true)
					.Where(pred).ToArray(),
				[AutoPropertyMode.Scene] = (property, pred) => BluEditor
					.GetAllComponentsInSceneOf(property.Context,
						property.Field.FieldType.GetElementType() ?? property.Field.FieldType)
					.Where(pred).ToArray(),
				[AutoPropertyMode.Asset] = (property, pred) => Resources
					.FindObjectsOfTypeAll(property.Field.FieldType.GetElementType() ?? property.Field.FieldType)
					.Where(AssetDatabase.Contains)
					.Where(pred).ToArray(),
				[AutoPropertyMode.Any] = (property, pred) => Resources
					.FindObjectsOfTypeAll(property.Field.FieldType.GetElementType() ?? property.Field.FieldType)
					.Where(pred).ToArray()
			};

		static AutoPropertyHandler()
		{
			// this event is for GameObjects in the project.
			BluEditorEvents.OnSave += CheckAssets;
			BluEditorEvents.BeforePlaymode += CheckAssets;
			// this event is for prefabs saved in edit mode.
			PrefabStage.prefabSaved += CheckComponentsInPrefab;
			PrefabStage.prefabStageOpened += stage => CheckComponentsInPrefab(stage.prefabContentsRoot);
		}

		private static void CheckAssets()
		{
			List<BluEditor.ObjectField> toFill = BluBoxSettings.EnableSOCheck ? 
				BluEditor.GetFieldsWithAttributeFromAll<AutoPropertyAttribute>() : 
				BluEditor.GetFieldsWithAttributeFromScenes<AutoPropertyAttribute>();
			toFill.ForEach(FillProperty);
		}

		private static void CheckComponentsInPrefab(GameObject prefab) => BluEditor
			.GetFieldsWithAttribute<AutoPropertyAttribute>(prefab)
			.ForEach(FillProperty);

		private static void FillProperty(BluEditor.ObjectField property)
		{
			AutoPropertyAttribute apAttribute = property.Field
				.GetCustomAttributes(typeof(AutoPropertyAttribute), true)
				.FirstOrDefault() as AutoPropertyAttribute;
			if (apAttribute == null) return;
			Func<Object, bool> predicateMethod = apAttribute.PredicateMethodTarget == null ?
				apAttribute.PredicateMethodName == null ?
				_ => true :
				(Func<Object, bool>)Delegate.CreateDelegate(typeof(Func<Object, bool>),
					property.Context,
					apAttribute.PredicateMethodName) :
				(Func<Object, bool>)Delegate.CreateDelegate(typeof(Func<Object, bool>),
					apAttribute.PredicateMethodTarget,
					apAttribute.PredicateMethodName);

			Object[] matchedObjects = objectsGetters[apAttribute.Mode]
				.Invoke(property, predicateMethod);

			if (property.Field.FieldType.IsArray)
			{
				if (matchedObjects != null && matchedObjects.Length > 0)
				{
					SerializedObject serializedObject = new(property.Context);
					SerializedProperty serializedProperty = serializedObject.FindProperty(property.Field.Name);
					serializedProperty.ReplaceArray(matchedObjects);
					serializedObject.ApplyModifiedProperties();
					return;
				}
			}
			else
			{
				Object obj = matchedObjects.FirstOrDefault();
				if (obj != null)
				{
					SerializedObject serializedObject = new(property.Context);
					SerializedProperty serializedProperty = serializedObject.FindProperty(property.Field.Name);
					serializedProperty.objectReferenceValue = obj;
					serializedObject.ApplyModifiedProperties();
					return;
				}
			}

			Debug.LogError($"{property.Context.name} caused: {property.Field.Name} is failed to Auto Assign property. No match",
				property.Context);
		}
	}
}
#endif
