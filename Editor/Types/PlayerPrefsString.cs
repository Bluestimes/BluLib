using System;
using BluLib.Internal;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public class PlayerPrefsString : PlayerPrefsType
	{
		public string Value
		{
			get => PlayerPrefs.GetString(Key, DefaultString);
			set => PlayerPrefs.SetString(Key, value);
		}

		public string DefaultString;
		
		public static PlayerPrefsString WithKey(string key, string defaultString = "") => new(key, defaultString);

		public PlayerPrefsString(string key, string defaultString = "")
		{
			Key = key;
			DefaultString = defaultString;
		}
	}
}