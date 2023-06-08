using System;
using BluLib.Internal;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public class PlayerPrefsFloat : PlayerPrefsType
	{
		public float Value
		{
			get => PlayerPrefs.GetFloat(Key, DefaultValue);
			set => PlayerPrefs.SetFloat(Key, value);
		}
		public float DefaultValue;
		
		
		public static PlayerPrefsFloat WithKey(string key, float defaultValue = 0) => new(key, defaultValue);

		public PlayerPrefsFloat(string key, float defaultValue = 0)
		{
			Key = key;
			DefaultValue = defaultValue;
		}
	}
}