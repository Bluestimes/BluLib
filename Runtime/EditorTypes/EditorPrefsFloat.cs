﻿#if UNITY_EDITOR
using System;
using UnityEditor;

namespace BluLib.EditorTools
{
	[Serializable]
	public class EditorPrefsFloat : EditorPrefsType
	{
		public float Value
		{
			get => EditorPrefs.GetFloat(Key, DefaultValue);
			set => EditorPrefs.SetFloat(Key, value);
		}

		public float DefaultValue;

		public static EditorPrefsFloat WithKey(string key, float defaultValue = 0) => new(key, defaultValue);
		
		public EditorPrefsFloat(string key, float defaultValue = 0)
		{
			Key = key;
			DefaultValue = defaultValue;
		} 
	}
}
#endif