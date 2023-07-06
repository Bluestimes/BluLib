using System;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public class AssetPath
	{
		public string Path;
		public string Extension;

		public static AssetPath WithExtension(string extension)
		{
			AssetPath assetPath = new();
			assetPath.Extension = extension;
			return assetPath;
		}
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(AssetPath))]
	public class AssetPathDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty pathProperty = property.FindPropertyRelative("Path");
			SerializedProperty extensionProperty = property.FindPropertyRelative("Extension");
			
			string name = label.text;
			string path = pathProperty.stringValue;

			int buttonWidth = 60;

			Rect textfieldBox = position;
			textfieldBox.width -= buttonWidth + 16;
			
			Rect buttonBox = position;
			buttonBox.width = buttonWidth;
			buttonBox.x = textfieldBox.width + 16;
			
			if (!path.Contains(Application.dataPath))
			{
				path = Application.dataPath;
			}

			string truncatedPath = path.Substring(Application.dataPath.Length);
			string filepath = Application.dataPath + EditorGUI.TextField(textfieldBox, name, truncatedPath);
			if (GUI.Button(buttonBox, "Browse"))
			{
				string newPath = EditorUtility.OpenFilePanel(name, path, extensionProperty.stringValue);
				if (newPath.Length > 0) filepath = newPath;
				
				if (!filepath.Contains(Application.dataPath))
				{
					Debug.LogError("Please choose a asset that is in this Assets Folder");

					pathProperty.stringValue = path;
					return;
				}
			}

			pathProperty.stringValue = filepath;
		}
	}
}
#endif