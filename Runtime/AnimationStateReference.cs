using System;
using System.Collections.Generic;
using System.Linq;
using BluLib.EditorTools;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BluLib
{
	[Serializable]
	public class AnimationStateReference
	{
		public string StateName => _stateName;

		public bool Assigned => _assigned;

		public Animator Animator => _linkedAnimator;

#pragma warning disable 0649
		[SerializeField] private string _stateName = string.Empty;
		[SerializeField] private bool _assigned;
		[SerializeField] private Animator _linkedAnimator;
#pragma warning restore 0649
	}

	public static class AnimationStateReferenceExtension
	{
		public static void Play(this Animator animator, AnimationStateReference state)
		{
			if (!state.Assigned) return;
			animator.Play(state.StateName);
		}
		
		public static void Play(this AnimationStateReference  state)
		{
			if (!state.Assigned) return;
			state.Animator.Play(state.StateName);
		}
	}
}

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [CustomPropertyDrawer(typeof(AnimationStateReference))]
	public class AnimationStateReferenceDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty stateNameProperty = property.FindPropertyRelative("_stateName");
			SerializedProperty assignedProperty = property.FindPropertyRelative("_assigned");
			SerializedProperty animatorProperty = property.FindPropertyRelative("_linkedAnimator");
			
			string[] states = GetStatesCollection();
			TryToAssignCurrentAnimator();
			
			

			float baseWidth = position.width - 4;

			Rect stateRect = position;
			stateRect.width = baseWidth / 4 * 3;

			Rect animatorRect = position;
			animatorRect.width = baseWidth / 4;
			animatorRect.x += stateRect.width + 4;


			EditorGUI.BeginProperty(position, label, property);
			int state = EditorGUI.Popup(stateRect, label, CurrentIndex(), states.Select(s => new GUIContent(s)).ToArray());
			stateNameProperty.stringValue = states[state];
			assignedProperty.boolValue = state > 0;

			EditorGUI.ObjectField(animatorRect, animatorProperty, GUIContent.none);
			EditorGUI.EndProperty();

			property.serializedObject.ApplyModifiedProperties();
			
			
			int CurrentIndex()
			{
				int index = states.IndexOfItem(stateNameProperty.stringValue);
				if (index < 0) index = 0;
				return index;
			}

			void TryToAssignCurrentAnimator()
			{
				if (animatorProperty.objectReferenceValue != null) return;
				
				MonoBehaviour mb = property.GetParent() as MonoBehaviour;
				if (mb == null) return;
				
				Animator animator = mb.GetComponentInChildren<Animator>(true);
				if (animator == null) return;
				
				animatorProperty.objectReferenceValue = animator;
				animatorProperty.serializedObject.ApplyModifiedProperties();
			}
			
			string[] GetStatesCollection()
			{
				if (animatorProperty.objectReferenceValue == null) return _empty;
				Animator animator = (Animator) animatorProperty.objectReferenceValue;
				AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
				if (controller == null)
				{
					AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
					if (overrideController == null) return _empty;

					controller = overrideController.runtimeAnimatorController as AnimatorController;
					if (controller == null) return _empty;
				}
				
				
				IEnumerable<string> statesInAnimator = controller.layers.SelectMany(l => l.stateMachine.states)
					.Select(s => (s.state.name)).Distinct();
				return _empty.Concat(statesInAnimator).ToArray();
			}
		}
		
		private const string DefaultState = "Not Assigned";
		private readonly string[] _empty = {DefaultState};
	}
}
#endif