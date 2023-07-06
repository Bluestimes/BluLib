#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using BluLib.EditorTools;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace BluLib.Internal
{
	[CustomEditor(typeof(AssetsPresetPreprocessBase))]
	public class AssetsPresetPreprocessEditor : Editor
	{
		[MenuItem("Tools/BluLib/Postprocess Preset Tool", false, 50)]
		private static void SelectBase()
		{
			AssetsPresetPreprocessBase presetBase = BluScriptableObject.LoadAssetsFromResources<AssetsPresetPreprocessBase>().FirstOrDefault();
			if (presetBase == null)
			{
				presetBase = BluScriptableObject.CreateAssetWithFolderDialog<AssetsPresetPreprocessBase>("AssetsPresetPostprocessBase");
			}

			if (presetBase != null) Selection.activeObject = presetBase;
		}

		private Vector2 _scrollPos;
		private GUIStyle _labelStyle;

		private AssetsPresetPreprocessBase _target;
		private ReorderableCollection _reorderableBase;
		private SerializedProperty _presets;
		private SerializedProperty _exclude;

		private void OnEnable()
		{
			_labelStyle = new(EditorStyles.label);
			_labelStyle.richText = true;

			_target = target as AssetsPresetPreprocessBase;
			
			_presets = serializedObject.FindProperty("Presets");
			_exclude = serializedObject.FindProperty("ExcludeProperties");
			_reorderableBase = new(_presets);

			_reorderableBase.CustomDrawerHeight += PresetDrawerHeight;
			_reorderableBase.CustomDrawer += PresetDrawer;
			_reorderableBase.CustomAdd += CustomAdd;
		}

		private void OnDisable()
		{
			if (_reorderableBase == null) return;
			_reorderableBase.CustomDrawerHeight -= PresetDrawerHeight;
			_reorderableBase.CustomDrawer -= PresetDrawer;
			_reorderableBase.CustomAdd -= CustomAdd;
			_reorderableBase = null;
		}

		private int PresetDrawerHeight(int index)
		{
			return (int) (EditorGUIUtility.singleLineHeight * 2 + 4);
		}

		private bool CustomAdd(int index)
		{
			EditorApplication.delayCall += () =>
			{
				SerializedProperty newElement = _presets.GetArrayElementAtIndex(index);
				newElement.FindPropertyRelative("PathContains").stringValue = string.Empty;
				newElement.FindPropertyRelative("TypeOf").stringValue = string.Empty;
				newElement.FindPropertyRelative("Prefix").stringValue = string.Empty;
				newElement.FindPropertyRelative("Postfix").stringValue = string.Empty;
				newElement.FindPropertyRelative("Preset").objectReferenceValue = null;
				newElement.serializedObject.ApplyModifiedProperties();
			};
			return false;
		}

		private void PresetDrawer(SerializedProperty property, Rect rect, int index)
		{
			PresetProperties properties = new(property);
			DrawPresetColourLine(rect, properties.Preset.objectReferenceValue as Preset);
			rect.width -= 6;
			rect.x += 6;


			EditorGUI.BeginChangeCheck();

			rect.height = EditorGUIUtility.singleLineHeight;
			int labelWidth = 24;
			int betweenFields = 6;

			Rect firstLineRect = new(rect);
			float flRatio = (rect.width - (labelWidth * 2 + betweenFields)) / 5;
			firstLineRect.width = flRatio * 3;

			EditorGUI.LabelField(firstLineRect, "PC:");
			firstLineRect.x += labelWidth;
			EditorGUI.PropertyField(firstLineRect, properties.PathContains, GUIContent.none);

			firstLineRect.x += firstLineRect.width + betweenFields;
			firstLineRect.width = flRatio * 2;
			EditorGUI.LabelField(firstLineRect, "FT:");
			firstLineRect.x += labelWidth;
			EditorGUI.PropertyField(firstLineRect, properties.TypeOf, GUIContent.none);


			rect.y += EditorGUIUtility.singleLineHeight + 2;
			Rect secondLineRect = new(rect);
			float slRatio = (rect.width - (labelWidth * 3 + betweenFields * 2)) / 10;

			float halfW = flRatio * 3 / 2 - (labelWidth / 2f) - (betweenFields / 2f);
			secondLineRect.width = halfW;
			EditorGUI.LabelField(secondLineRect, "Pr:");
			secondLineRect.x += labelWidth;
			EditorGUI.PropertyField(secondLineRect, properties.Prefix, GUIContent.none);

			secondLineRect.x += secondLineRect.width + betweenFields;
			secondLineRect.width = halfW;
			EditorGUI.LabelField(secondLineRect, "Po:");
			secondLineRect.x += labelWidth;
			EditorGUI.PropertyField(secondLineRect, properties.Postfix, GUIContent.none);

			secondLineRect.x += secondLineRect.width + betweenFields;
			secondLineRect.width = slRatio * 4;
			secondLineRect.x += labelWidth;
			
			EditorGUI.PropertyField(secondLineRect, properties.Preset, GUIContent.none);


			if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();
		}


		private struct PresetProperties
		{
			public readonly SerializedProperty PathContains;
			public readonly SerializedProperty TypeOf;
			public readonly SerializedProperty Prefix;
			public readonly SerializedProperty Postfix;

			public readonly SerializedProperty Preset;

			public PresetProperties(SerializedProperty baseProperty)
			{
				PathContains = baseProperty.FindPropertyRelative("PathContains");
				TypeOf = baseProperty.FindPropertyRelative("TypeOf");
				Prefix = baseProperty.FindPropertyRelative("Prefix");
				Postfix = baseProperty.FindPropertyRelative("Postfix");
				Preset = baseProperty.FindPropertyRelative("Preset");
			}
		}

		private void DrawPresetColourLine(Rect rect, Preset preset)
		{
			Rect cRect = new(rect);
			cRect.width = 6;
			cRect.height -= 2;

			Color color = BluGUI.Colors.Brown;
			if (preset == null) color = Color.red;
			else
			{
				string presetType = preset.GetTargetTypeName();
				if (presetType.Contains("Texture")) color = BluGUI.Colors.Blue;
				else if (presetType.Contains("Audio")) color = BluGUI.Colors.Red;
			}

			BluGUI.DrawColouredRect(cRect, color);
			EditorGUI.LabelField(cRect, GUIContent.none);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("First match will be applied");
			EditorGUILayout.LabelField("Assets/...<b>[PC:Path Contains]</b>.../", _labelStyle);
			EditorGUILayout.LabelField("<b>[Pr:Prefix]</b>...<b>[Po:Postfix]</b>.<b>[FT:File Type]</b>", _labelStyle);
			EditorGUILayout.Space();

			_scrollPos = GUILayout.BeginScrollView(_scrollPos);

			_reorderableBase.Draw();

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(_exclude, true);
			if (EditorGUI.EndChangeCheck())
			{
				EditorApplication.delayCall += UpdateExcludes;
			}

			EditorGUILayout.Space();

			if (GUILayout.Button("Update Excludes", EditorStyles.toolbarButton)) UpdateExcludes();

			GUILayout.EndScrollView();

			serializedObject.ApplyModifiedProperties();
		}

		private void UpdateExcludes()
		{
			foreach (ConditionalPreset preset in _target.Presets)
			{
				if (preset.Preset == null) continue;

				UpdateExcludesOnPreset(preset);
			}
		}

		private void UpdateExcludesOnPreset(ConditionalPreset preset)
		{
			List<string> toApply = new();
			foreach (PropertyModification modification in preset.Preset.PropertyModifications)
			{
				string path = modification.propertyPath;
				bool exclude = false;
				for (int i = 0; i < _target.ExcludeProperties.Length; i++)
				{
					string excludePath = _target.ExcludeProperties[i];
					if (path.Contains(excludePath))
					{
						exclude = true;
						break;
					}
				}
				if (!exclude) toApply.Add(path);

				serializedObject.ApplyModifiedProperties();
			}

			preset.PropertiesToApply = toApply.ToArray();
			EditorUtility.SetDirty(target);
		}
	}
}
#endif