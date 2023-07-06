using System;
using BluLib.EditorTools;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public struct BluCursor
	{
		public Texture2D Texture;
		public Vector2 Hotspot;

		public void ApplyAsLockedCursor()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.SetCursor(Texture, Hotspot, CursorMode.ForceSoftware);
		}
		
		public void ApplyAsFreeCursor()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			Cursor.SetCursor(Texture, Hotspot, CursorMode.ForceSoftware);
		}
		
		public void ApplyAsConfinedCursor()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.SetCursor(Texture, Hotspot, CursorMode.ForceSoftware);
		}
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(BluCursor), true)]
	public class BluCursorPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty texture = property.FindPropertyRelative("Texture");
			SerializedProperty hotspot = property.FindPropertyRelative("Hotspot");
			
			position.width -= 128;
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(position, texture, label);
			if (EditorGUI.EndChangeCheck()) hotspot.vector2Value = Vector2.zero;
			// position.x += position.width + 4;
			// position.width = 48;
			// EditorGUI.LabelField(position, "Hotspot: ");

			position.x += position.width + 4;
			position.width = 84;
			EditorGUI.PropertyField(position, hotspot, new GUIContent(string.Empty, "Hotspot"));
			
			position.x += position.width + 4;
			position.width = 34;
			GUI.enabled = texture.objectReferenceValue != null;
			if (GUI.Button(position, "Edit"))
			{
				PopupWindow.Show(position, new HotspotEditorPopup(property));
			}
			GUI.enabled = true;
			
			hotspot.serializedObject.ApplyModifiedProperties();
		}
	}
	
	public class HotspotEditorPopup : PopupWindowContent
	{
		private readonly SerializedProperty _hotspotProperty;
		private readonly Texture2D _texture;
		
		public HotspotEditorPopup(SerializedProperty property)
		{
			SerializedProperty textureProperty = property.FindPropertyRelative("Texture");
			_hotspotProperty = property.FindPropertyRelative("Hotspot");
			_texture = textureProperty.objectReferenceValue as Texture2D;
		}

		public override Vector2 GetWindowSize()
		{
			int width = _texture.width + 8;
			int height = _texture.height + 78;
			return new(width < 128 ? 128 : width, height < 172 ? 172 : height);
		}

		public override void OnGUI(Rect rect)
		{
			if (_texture == null) return;
			
			GUILayout.Label(_texture, GUILayout.Width(_texture.width), GUILayout.Height(_texture.height));
			Rect textureRect = GUILayoutUtility.GetLastRect();
			
			Event e = Event.current;
			if (e.type == EventType.MouseDrag)
			{
				Vector2 pos = Event.current.mousePosition;
				if (pos.y > textureRect.yMax + 10) return;
				Vector2 inside = new(pos.x - textureRect.x, pos.y - textureRect.y);
				inside = new(
					Mathf.Clamp(inside.x, 0, _texture.width),
					Mathf.Clamp(inside.y, 0, _texture.height)
				);
				_hotspotProperty.vector2Value = inside;
				_hotspotProperty.serializedObject.ApplyModifiedProperties();
				editorWindow.Repaint();
			}

			Color cc = GUI.contentColor;
			GUI.contentColor = Color.magenta;
			textureRect.width = 8;
			textureRect.height = 8;
			textureRect.x += _hotspotProperty.vector2Value.x - 4;
			textureRect.y += _hotspotProperty.vector2Value.y - 4;
			GUI.Label(textureRect, Texture2D.whiteTexture);
			GUI.contentColor = cc;

			EditorGUILayout.Space();
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.Space();
				if (GUILayout.Button("↖", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(0, 0);
				if (GUILayout.Button("↑", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(_texture.width / 2f, 0);
				if (GUILayout.Button("↗", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(_texture.width, 0);
				EditorGUILayout.Space();
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.Space();
				if (GUILayout.Button("←", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(0, _texture.height / 2f);
				if (GUILayout.Button(BluGUI.Characters.Cross, EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(_texture.width / 2f, _texture.height / 2f);
				if (GUILayout.Button("→", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(_texture.width, _texture.height / 2f);
				EditorGUILayout.Space();
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.Space();
				if (GUILayout.Button("↙", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(0, _texture.height);
				if (GUILayout.Button("↓", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(_texture.width / 2f, _texture.height);
				if (GUILayout.Button("↘", EditorStyles.toolbarButton, GUILayout.Width(40)))
					_hotspotProperty.vector2Value = new(_texture.width, _texture.height);
				EditorGUILayout.Space();
			}

			if (GUI.changed)
			{
				_hotspotProperty.serializedObject.ApplyModifiedProperties();
				editorWindow.Repaint();
			}
		}
	}
}
#endif
