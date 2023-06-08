using UnityEditor;
using UnityEngine;

namespace BluLib
{
	/// <summary>
	/// Pivot and Anchor of Target makes no difference.
	/// Current Pivot and "Target Anchor" property used for positioning.
	/// </summary>
	[ExecuteInEditMode]
	public class UIRelativePosition : MonoBehaviour
	{
		[MustBeAssigned] public RectTransform Target;

		[Separator("Set X/Y, with optional offset")]
		public OptionalFloat SetX = OptionalFloat.WithValue(0);

		public OptionalFloat SetY = OptionalFloat.WithValue(0);

		[Separator("0-1 point on Target rect")]
		public Vector2 TargetAnchor = new(.5f, .5f);


		private RectTransform _transform;
		private Vector2 _latestSize;
		private Vector3 _latestPosition;
		private bool _firstCall;
		
		private void Start()
		{
			_transform = transform as RectTransform;

			if (_transform == null) Debug.LogError(name + " Caused: Transform is not a RectTransform", this);
			if (!SetX.IsSet && !SetY.IsSet) Debug.LogError(name + " Caused: Check SetX and/or SetY for RelativePosition to work", this);
		}

		private void LateUpdate()
		{
			if (_transform == null) return;
			if (Target == null) return;
			if (!_firstCall)
			{
				// Position is zero on PrefabModeEntered?
				// ForceUpdateRectTransforms is not helping, but on second frame it's all ok
				_firstCall = true;
				return;
			}
			
			Vector2 relativeToSize = Target.sizeDelta;
			Vector3 relativeToPosition = Target.position;
			if (_latestSize == relativeToSize && _latestPosition == relativeToPosition) return;
			_latestSize = relativeToSize;
			_latestPosition = relativeToPosition;

			Vector3 scale = Target.lossyScale;
			Vector2 pivot = Target.pivot;
			float anchorOffsetX = relativeToSize.x * TargetAnchor.x;
			float anchorOffsetY = relativeToSize.y * TargetAnchor.y;
			float left = relativeToPosition.x - (relativeToSize.x * pivot.x * scale.x);
			float top = relativeToPosition.y + relativeToSize.y - (relativeToSize.y * pivot.y * scale.y);
			float x = left + anchorOffsetX + SetX.Value;
			float y = top - anchorOffsetY + SetY.Value;

			Vector3 localPosition = _transform.position;
			Vector2 finalPosition = new(SetX.IsSet ? (int)x : localPosition.x, SetY.IsSet ? (int)y : localPosition.y);
			_transform.position = finalPosition;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			UpdateView();
		}

		[ButtonMethod]
		private void UpdateView()
		{
			_latestSize = Vector2.zero;
			_transform = transform as RectTransform;
			if (_transform == null) return;
			
			Undo.RecordObject(_transform, "UIRelativePosition.UpdateView");
			LateUpdate();
		}
#endif
	}
}