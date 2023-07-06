using System;
using BluLib.EditorTools;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public class TransformData
	{
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Scale;

		public bool SavePosition = true;
		public bool SaveRotation = true;
		public bool SaveScale = true;

		public Action OnSaved;
		public Action OnApplied;

		public void Apply(Transform transform)
		{
			if (SavePosition) transform.position = Position;
			if (SaveRotation) transform.rotation = Quaternion.Euler(Rotation);
			if (SaveScale) transform.localScale = Scale;

			OnApplied?.Invoke();
		}

		public void Save(Transform transform)
		{
			Position = transform.position;
			Rotation = transform.rotation.eulerAngles;
			Scale = transform.localScale;
			OnSaved?.Invoke();
		}

		public static TransformData FromTransform(Transform transform, bool savePosition = true, bool saveRotation = true, bool saveScale = true)
		{
			TransformData data = new();
			data.Save(transform);
			data.SavePosition = savePosition;
			data.SaveRotation = saveRotation;
			data.SaveScale = saveScale;
			return data;
		}
	}
}


#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(TransformData))]
	public class TransformDataDrawer : PropertyDrawer
	{
		private static GUIContent PositionIcon =>
			_positionIcon ?? (_positionIcon = new(EditorGUIUtility.IconContent("MoveTool").image, "Save Position"));

		private static GUIContent _positionIcon;

		private static GUIContent RotationIcon =>
			_rotationIcon ?? (_rotationIcon = new(EditorGUIUtility.IconContent("RotateTool").image, "Save Rotation"));

		private static GUIContent _rotationIcon;

		private static GUIContent ScaleIcon =>
			_scaleIcon ?? (_scaleIcon = new(EditorGUIUtility.IconContent("ScaleTool").image, "Save Scale"));

		private static GUIContent _scaleIcon;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty positionProperty = property.FindPropertyRelative(nameof(TransformData.Position));
			SerializedProperty rotationProperty = property.FindPropertyRelative(nameof(TransformData.Rotation));
			SerializedProperty scaleProperty = property.FindPropertyRelative(nameof(TransformData.Scale));

			SerializedProperty savePosition = property.FindPropertyRelative(nameof(TransformData.SavePosition));
			SerializedProperty saveRotation = property.FindPropertyRelative(nameof(TransformData.SaveRotation));
			SerializedProperty saveScale = property.FindPropertyRelative(nameof(TransformData.SaveScale));

			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(position, label);

			Rect bRect = position;
			bRect.x += bRect.width - 220;
			bRect.width = 30;
			savePosition.boolValue = GUI.Toggle(bRect, savePosition.boolValue, PositionIcon, EditorStyles.miniButtonLeft);
			bRect.x += 30;
			saveRotation.boolValue = GUI.Toggle(bRect, saveRotation.boolValue, RotationIcon, EditorStyles.miniButtonMid);
			bRect.x += 30;
			saveScale.boolValue = GUI.Toggle(bRect, saveScale.boolValue, ScaleIcon, EditorStyles.miniButtonRight);

			MonoBehaviour mb = property.GetParent() as MonoBehaviour;
			using (new ConditionallyEnabledGUIBlock(mb != null))
			{
				bRect.x += 44;
				bRect.width = 56;
				if (GUI.Button(bRect, "Bake") && mb != null)
				{
					Transform owner = mb.transform;
					TransformData data = (TransformData) property.GetValue();
					data.Save(owner);
					EditorUtility.SetDirty(owner);
				}

				bRect.x += 60;
				if (GUI.Button(bRect, "Restore") && mb != null)
				{
					Transform owner = mb.transform;
					TransformData data = (TransformData) property.GetValue();
					data.Apply(owner);
					EditorUtility.SetDirty(owner);
				}
			}

			position.x += 20;
			position.width -= 20;
			if (savePosition.boolValue)
			{
				position.y += position.height;
				EditorGUI.PropertyField(position, positionProperty);
			}

			if (saveRotation.boolValue)
			{
				position.y += position.height;
				EditorGUI.PropertyField(position, rotationProperty);
			}

			if (saveScale.boolValue)
			{
				position.y += position.height;
				EditorGUI.PropertyField(position, scaleProperty);
			}

			property.serializedObject.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty savePosition = property.FindPropertyRelative(nameof(TransformData.SavePosition));
			SerializedProperty saveRotation = property.FindPropertyRelative(nameof(TransformData.SaveRotation));
			SerializedProperty saveScale = property.FindPropertyRelative(nameof(TransformData.SaveScale));

			float height = EditorGUIUtility.singleLineHeight;
			if (savePosition.boolValue) height += EditorGUIUtility.singleLineHeight + 2;
			if (saveRotation.boolValue) height += EditorGUIUtility.singleLineHeight + 2;
			if (saveScale.boolValue) height += EditorGUIUtility.singleLineHeight + 2;

			return height;
		}
	}
}
#endif