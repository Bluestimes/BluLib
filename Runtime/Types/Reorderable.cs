using System;
using System.Collections.Generic;
using BluLib.EditorTools;
using BluLib.Internal;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	#region Default Reordable Types

	[Serializable]
	public class ReorderableGameObject : Reorderable<GameObject>
	{
	}

	[Serializable]
	public class ReorderableGameObjectList : ReorderableList<GameObject>
	{
	}

	[Serializable]
	public class ReorderableTransform : Reorderable<Transform>
	{
	}

	[Serializable]
	public class ReorderableTransformList : ReorderableList<Transform>
	{
	}

	#endregion


	[Serializable]
	public class Reorderable<T> : ReorderableBase
	{
		public T[] Collection;

		public int Length
		{
			get { return Collection.Length; }
		}
		
		public T this[int i]
		{
			get { return Collection[i]; }
			set { Collection[i] = value; }
		}
	}

	[Serializable]
	public class ReorderableList<T> : ReorderableBase
	{
		public List<T> Collection;
	}
}

namespace BluLib.Internal
{
	[Serializable]
	public class ReorderableBase
	{
	}
}


#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(ReorderableBase), true)]
	public class ReorderableTypePropertyDrawer : PropertyDrawer
	{
		private ReorderableCollection _reorderable;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (_reorderable == null)
				_reorderable = new(property.FindPropertyRelative("Collection"), true, true, property.displayName);
			
			return _reorderable != null ? _reorderable.Height : base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			_reorderable.Draw(position);
			EditorGUI.EndProperty();
		}
	}
}
#endif