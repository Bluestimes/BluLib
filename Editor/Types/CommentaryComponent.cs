using System;
using BluLib.EditorTools;
using UnityEditor;
using UnityEngine;

namespace BluLib.Internal
{
	public class CommentaryComponent : MonoBehaviour
	{
#if UNITY_EDITOR
		[Serializable]
		public struct Entry
		{
			public string EditorCommentary;
			public MessageType Type;
		}

		public Entry[] Entries;
#endif
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomEditor(typeof(CommentaryComponent))]
	public class CommentaryDrawer : Editor
	{
		private CommentaryComponent _commentary;
		private GUIContent _boxContent;

		private bool _editMode;

		public override void OnInspectorGUI()
		{
			if (_commentary == null) _commentary = target as CommentaryComponent;
			if (_commentary == null) return;
			if (_commentary.Entries == null)
			{
				_commentary.Entries = new CommentaryComponent.Entry[0];
				EditorUtility.SetDirty(_commentary);
			}

			if (!_editMode && _commentary.Entries.Length > 0)
			{
				Event e = Event.current;
				foreach (CommentaryComponent.Entry entry in _commentary.Entries)
				{
					EditorGUILayout.HelpBox(entry.EditorCommentary, entry.Type);
					if (GUILayoutUtility.GetLastRect().Contains(e.mousePosition) && e.button == 0 && e.isMouse) _editMode = true;
				}

				if (_editMode) return;
			}

			if (_editMode || _commentary.Entries.Length == 0)
			{
				for (int i = 0; i < _commentary.Entries.Length; i++)
				{
					CommentaryComponent.Entry entry = _commentary.Entries[i];
					using (new EditorGUILayout.HorizontalScope())
					{
						using (new GUILayout.VerticalScope(GUILayout.Width(40)))
						{
							GUILayout.Space(4);
							if (GUILayout.Button(GetIcon(entry.Type), EditorStyles.helpBox, GUILayout.Width(40), GUILayout.Height(36)))
							{
								_commentary.Entries[i].Type = NextType(entry.Type);
							}

							if (GUILayout.Button(BluGUI.Characters.Cross, GUILayout.Width(40)))
							{
								int index = i;
								EditorApplication.delayCall += () =>
								{
									_commentary.Entries = _commentary.Entries.RemoveAt(index);
									EditorUtility.SetDirty(_commentary);
									Repaint();
								};
							}
						}

						_commentary.Entries[i].EditorCommentary = GUILayout.TextArea(entry.EditorCommentary, EditorStyles.helpBox);
					}
				}

				EditorGUILayout.Space();
				using (new GUILayout.HorizontalScope())
				{
					if (_commentary.Entries.Length > 0 && GUILayout.Button(BluGUI.Characters.Check, GUILayout.Width(40))) _editMode = false;
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("+", GUILayout.Width(40)))
					{
						Array.Resize(ref _commentary.Entries, _commentary.Entries.Length + 1);
						_editMode = true;
						EditorUtility.SetDirty(_commentary);
					}
				}


				if (GUI.changed) EditorUtility.SetDirty(target);
			}
			
		}

		private GUIContent GetIcon(MessageType type)
		{
			if (type == MessageType.Info) return EditorGUIUtility.IconContent("console.infoicon");
			if (type == MessageType.Warning) return EditorGUIUtility.IconContent("console.warnicon");
			if (type == MessageType.Error) return EditorGUIUtility.IconContent("console.erroricon");
			return new("No icon");
		}

		private MessageType NextType(MessageType type)
		{
			if (type == MessageType.Info) return MessageType.Warning;
			if (type == MessageType.Warning) return MessageType.Error;
			if (type == MessageType.Error) return MessageType.None;
			return MessageType.Info;
		}
	}
}
#endif