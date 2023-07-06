using System;
using BluLib.Internal;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public class PlayerPrefsVector3 : PlayerPrefsType
	{
		public Vector3 Value
		{
			get => new(
				PlayerPrefs.GetFloat(Key+"x", DefaultValue.x), 
				PlayerPrefs.GetFloat(Key+"y", DefaultValue.y), 
				PlayerPrefs.GetFloat(Key+"z", DefaultValue.z));
			set
			{
				PlayerPrefs.SetFloat(Key+"x", value.x);
				PlayerPrefs.SetFloat(Key+"y", value.y);
				PlayerPrefs.SetFloat(Key+"z", value.z);
			}
		}
		public Vector3 DefaultValue;
		
		public static PlayerPrefsVector3 WithKey(string key, Vector3 defaultValue = new()) => new(key, defaultValue);

		public PlayerPrefsVector3(string key, Vector3 defaultValue = new())
		{
			Key = key;
			DefaultValue = defaultValue;
		}
	}
}