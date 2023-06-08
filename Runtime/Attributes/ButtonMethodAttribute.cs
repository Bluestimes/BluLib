// ---------------------------------------------------------------------------- 
// Author: Kaynn, Yeo Wen Qin
// https://github.com/Kaynn-Cahya
// Date:   26/02/2019
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BluLib.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BluLib
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ButtonMethodAttribute : PropertyAttribute
	{
		public readonly ButtonMethodDrawOrder DrawOrder;
		public readonly ConditionalData Condition;
		
		public ButtonMethodAttribute(ButtonMethodDrawOrder drawOrder = ButtonMethodDrawOrder.AfterInspector) => DrawOrder = drawOrder;

		public ButtonMethodAttribute(ButtonMethodDrawOrder drawOrder, string fieldToCheck, bool inverse = false, params object[] compareValues)
			=> (DrawOrder, Condition) = (drawOrder, new(fieldToCheck, inverse, compareValues));

		public ButtonMethodAttribute(ButtonMethodDrawOrder drawOrder, string[] fieldToCheck, bool[] inverse = null, params object[] compare)
			=> (DrawOrder, Condition) = (drawOrder, new(fieldToCheck, inverse, compare));

		public ButtonMethodAttribute(ButtonMethodDrawOrder drawOrder, params string[] fieldToCheck) 
			=> (DrawOrder, Condition) = (drawOrder, new(fieldToCheck));
		
		public ButtonMethodAttribute(ButtonMethodDrawOrder drawOrder, bool useMethod, string method, bool inverse = false) 
			=> (DrawOrder, Condition) = (drawOrder, new(useMethod, method, inverse));
	}

	public enum ButtonMethodDrawOrder
	{
		BeforeInspector, 
		AfterInspector
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    public class ButtonMethodHandler
	{
		public readonly List<(MethodInfo Method, string Name, ButtonMethodDrawOrder Order, ConditionalData Condition)> TargetMethods;
		public int Amount => TargetMethods?.Count ?? 0;
		
		private readonly Object target;

		public ButtonMethodHandler(Object target)
		{
			this.target = target;
			
			Type type = target.GetType();
			BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			IEnumerable<MemberInfo> members = type.GetMembers(bindings).Where(IsButtonMethod);

			foreach (MemberInfo member in members)
			{
				MethodInfo method = member as MethodInfo;
				if (method == null) continue;
				
				if (IsValidMember(method, member))
				{
					ButtonMethodAttribute attribute = (ButtonMethodAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonMethodAttribute));
					if (TargetMethods == null) TargetMethods = new List<(MethodInfo, string, ButtonMethodDrawOrder, ConditionalData)>();
					TargetMethods.Add((method, method.Name.SplitCamelCase(), attribute.DrawOrder, attribute.Condition));
				}
			}
		}

		public void OnBeforeInspectorGUI()
		{
			if (TargetMethods == null) return;
			
			bool anyDrawn = false;
			foreach ((MethodInfo Method, string Name, ButtonMethodDrawOrder Order, ConditionalData Condition) method in TargetMethods)
			{
				if (method.Order != ButtonMethodDrawOrder.BeforeInspector) continue;
				if (method.Condition != null && !ConditionalUtility.IsConditionMatch(target, method.Condition)) return;
				
				anyDrawn = true;
				if (GUILayout.Button(method.Name)) InvokeMethod(target, method.Method);
			}
			
			if (anyDrawn) EditorGUILayout.Space();
		}

		public void OnAfterInspectorGUI()
		{
			if (TargetMethods == null) return;
			bool anyDrawn = false;

			foreach ((MethodInfo Method, string Name, ButtonMethodDrawOrder Order, ConditionalData Condition) method in TargetMethods)
			{
				if (method.Order != ButtonMethodDrawOrder.AfterInspector) continue;
				if (method.Condition != null && !ConditionalUtility.IsConditionMatch(target, method.Condition)) return;
				
				if (!anyDrawn)
				{
					EditorGUILayout.Space();
					anyDrawn = true;
				}
				
				if (GUILayout.Button(method.Name)) InvokeMethod(target, method.Method);
			}
		}
		
		public void Invoke(MethodInfo method) => InvokeMethod(target, method);

		
		private void InvokeMethod(Object target, MethodInfo method)
		{
			object result = method.Invoke(target, null);

			if (result != null)
			{
				string message = $"{result} \nResult of Method '{method.Name}' invocation on object {target.name}";
				Debug.Log(message, target);
			}
		}
		
		private bool IsButtonMethod(MemberInfo memberInfo)
		{
			return Attribute.IsDefined(memberInfo, typeof(ButtonMethodAttribute));
		}
			
		private bool IsValidMember(MethodInfo method, MemberInfo member)
		{
			if (method == null)
			{
				Debug.LogWarning(
					$"Property <color=brown>{member.Name}</color>.Reason: Member is not a method but has EditorButtonAttribute!");
				return false;
			}

			if (method.GetParameters().Length > 0)
			{
				Debug.LogWarning(
					$"Method <color=brown>{method.Name}</color>.Reason: Methods with parameters is not supported by EditorButtonAttribute!");
				return false;
			}

			return true;
		}
	}
}
#endif