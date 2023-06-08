#if UNITY_IMAGECONVERSION_ENABLED
using System;
using UnityEngine;

namespace BluLib
{
	public static class ImageStringConverter
	{
		/// <summary>
		/// Use "Tools/BluLib/String Image Converter" to get string image representation
		/// </summary>
		public static Texture2D ImageFromString(string source, int width, int height)
		{
			byte[] bytes = Convert.FromBase64String(source);
			Texture2D texture = new(width, height);
			texture.LoadImage(bytes);
			return texture;
		}
	}
}
#endif