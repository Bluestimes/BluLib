#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace BluLib.EditorTools
{	
	[Serializable]
	public class EditorPrefsVector3 : EditorPrefsType
	{
		public Vector3 Value
		{
			get => new(
				EditorPrefs.GetFloat(Key+"x", DefaultValue.x), 
				EditorPrefs.GetFloat(Key+"y", DefaultValue.y), 
				EditorPrefs.GetFloat(Key+"z", DefaultValue.z));
			set
			{
				EditorPrefs.SetFloat(Key+"x", value.x);
				EditorPrefs.SetFloat(Key+"y", value.y);
				EditorPrefs.SetFloat(Key+"z", value.z);
			}
		}

		public Vector3 DefaultValue;
		
		public static EditorPrefsVector3 WithKey(string key, Vector3 defaultValue = new()) => new(key, defaultValue);
		
		public EditorPrefsVector3(string key, Vector3 defaultValue = new())
		{
			Key = key;
			DefaultValue = defaultValue;
		}
	}
}
#endif