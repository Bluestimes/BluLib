#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace BluLib.Internal
{
	[InitializeOnLoad]
	public class MustBeAssignedConditionalFieldExclude
	{
		static MustBeAssignedConditionalFieldExclude()
		{
			MustBeAssignedAttributeChecker.ExcludeFieldFilter += ExcludeCheckIfConditionalFieldHidden;
		}
		
		private static readonly Type conditionalType = typeof(ConditionalFieldAttribute);
		
		private static bool ExcludeCheckIfConditionalFieldHidden(FieldInfo field, Object obj)
		{
			if (conditionalType == null) return false;
			if (!field.IsDefined(conditionalType, false)) return false;

			// Get a specific attribute of this field
			ConditionalFieldAttribute conditional = field.GetCustomAttributes(conditionalType, false)
				.Select(a => a as ConditionalFieldAttribute)
				.SingleOrDefault();

			return conditional != null && !ConditionalUtility.IsConditionMatch(obj, conditional.Data);
		}
	}
}
#endif