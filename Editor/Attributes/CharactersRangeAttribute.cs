using System.Collections.Generic;
using System.Linq;
using BluLib.EditorTools;
using UnityEditor;
using UnityEngine;

namespace BluLib
{
	/// <summary>
	/// Validate a string field to only allow or disallow a set of pre-defined
	/// characters on typing.
	/// </summary>
	public class CharactersRangeAttribute : PropertyAttribute
	{
		public readonly string Characters;
		public readonly CharacterRangeMode Mode;
		public readonly bool IgnoreCase;

		public CharactersRangeAttribute(string characters, CharacterRangeMode mode = CharacterRangeMode.Allow,
			bool ignoreCase = true)
		{
			Characters = characters;
			Mode = mode;
			IgnoreCase = ignoreCase;
		}
	}

	public enum CharacterRangeMode
	{
		/// <summary>
		/// Only characters in range will be allowed
		/// </summary>
		Allow,
		/// <summary>
		/// Characters in range will be removed from the string
		/// </summary>
		Disallow,
		/// <summary>
		/// Highlight the field if any of the specified characters were fould
		/// </summary>
		WarningIfAny,
		/// <summary>
		/// Highlight the field if some characters in string are not the characters from specified range
		/// </summary>
		WarningIfNotMatch
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(CharactersRangeAttribute))]
	public class CharacterRangeAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				BluGUI.DrawColouredRect(position, BluGUI.Colors.Red);
				EditorGUI.LabelField(position, new GUIContent("", "[CharacterRangeAttribute] used with non-string property"));
			}
			else
			{
				CharactersRangeAttribute charactersRange = (CharactersRangeAttribute)attribute;
				CharacterRangeMode mode = charactersRange.Mode;
				bool ignoreCase = charactersRange.IgnoreCase;
				string filter = charactersRange.Characters;
				bool allow = mode == CharacterRangeMode.Allow || mode == CharacterRangeMode.WarningIfNotMatch;
				bool warning = mode == CharacterRangeMode.WarningIfAny || mode == CharacterRangeMode.WarningIfNotMatch;

				if (ignoreCase) filter = filter.ToUpper();
				IEnumerable<char> filteredCharacters = property.stringValue.Distinct()
					.Where(c =>
					{
						if (ignoreCase) c = char.ToUpper(c);
						return filter.Contains(c) ^ allow;
					});
				bool ifMatch = mode == CharacterRangeMode.WarningIfAny;
				bool ifNotMatch = mode == CharacterRangeMode.WarningIfNotMatch;
				bool anyFiltered = filteredCharacters.Any();
				bool warn = (ifMatch && anyFiltered || ifNotMatch && anyFiltered);
				Rect originalPosition = position;

				DrawWarning();
				position.width -= 20;

				property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
				DrawTooltip();

				if (!warning)
				{
					property.stringValue = filteredCharacters.Aggregate(
						property.stringValue,
						(p, c) => p.Replace(c.ToString(), ""));
				}

				if (warn)
				{
					GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
					position = originalPosition;
					position.y += EditorGUIUtility.singleLineHeight;
					DrawWarning();
					position.x += EditorGUIUtility.labelWidth;
					GUIContent warningContent = new("Containing disallowed characters!");
					EditorGUI.LabelField(position, warningContent, EditorStyles.miniBoldLabel);
				}

				property.serializedObject.ApplyModifiedProperties();


				void DrawWarning()
				{
					if (!warning) return;
					if (property.stringValue.Length == 0) warn = false;
					BluGUI.DrawColouredRect(position, warn ? BluGUI.Colors.Yellow : Color.clear);
				}

				void DrawTooltip()
				{
					string tooltip = "Characters range ";
					if (mode == CharacterRangeMode.Allow || mode == CharacterRangeMode.WarningIfNotMatch) tooltip += "is allowed:";
					else tooltip += "not allowed:";
					tooltip += $"\n[{filter}]";

					position.x += position.width + 2;
					position.width = 18;
					GUIContent tooltipContent = new(BluGUI.EditorIcons.Help);
					tooltipContent.tooltip = tooltip;
					EditorGUI.LabelField(position, tooltipContent, EditorStyles.label);
				}
			}
		}
	}
}
#endif