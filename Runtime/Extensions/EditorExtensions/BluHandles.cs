#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace BluLib.EditorTools
{
	public static class BluHandles
	{
		/// <summary>
		/// Draw line with arrows showing direction
		/// </summary>
		public static void DrawDirectionalLine(Vector3 fromPos, Vector3 toPos, float screenSpaceSize = 3, float arrowsDensity = .5f)
		{
			float arrowSize = screenSpaceSize / 4;

			Handles.DrawLine(fromPos, toPos);

			Vector3 direction = toPos - fromPos;
			float distance = Vector3.Distance(fromPos, toPos);
			int arrowsCount = (int) (distance / arrowsDensity);
			float delta = 1f / arrowsCount;
			for (int i = 1; i <= arrowsCount; i++)
			{
				float currentDelta = delta * i;
				Vector3 currentPosition = Vector3.Lerp(fromPos, toPos, currentDelta);
				DrawTinyArrow(currentPosition, direction, arrowSize);
			}
		}

		/// <summary>
		/// Draw arrow in position to direction
		/// </summary>
		public static void DrawTinyArrow(Vector3 position, Vector3 direction, float headLength = 0.25f, float headAngle = 20.0f)
		{
			Quaternion lookRotation = Quaternion.LookRotation(direction);
			Vector3 rightVector = new(0, 0, 1);
			Vector3 right = lookRotation * Quaternion.Euler(0, 180 + headAngle, 0) * rightVector;
			Vector3 left = lookRotation * Quaternion.Euler(0, 180 - headAngle, 0) * rightVector;
			Handles.DrawLine(position, position + right * headLength);
			Handles.DrawLine(position, position + left * headLength);
		}

#if UNITY_AI_ENABLED
        
		/// <summary>
		/// Draw arrowed gizmo in scene view to visualize path
		/// </summary>
		public static void VisualizePath(NavMeshPath path)
		{
			Vector3[] corners = path.corners;
			for (int i = 1; i < corners.Length; i++)
			{
				Vector3 cornerA = corners[i - 1];
				Vector3 cornerB = corners[i];
				float size = HandleUtility.GetHandleSize(new(cornerA.x, cornerA.y, cornerA.z));
				DrawDirectionalLine(cornerA, cornerB, size, size);
			}
		}
        
#endif

		/// <summary>
		/// Draw flying path of height prom pointA to pointB
		/// </summary>
		public static void DrawFlyPath(Vector3 pointA, Vector3 pointB, float height = 3)
		{
			Color color = Handles.color;
			Vector3 pointAOffset = new(pointA.x, pointA.y + height, pointA.z);
			Vector3 pointBOffset = new(pointB.x, pointB.y + height, pointB.z);
			Handles.DrawBezier(pointA, pointB, pointAOffset, pointBOffset, color, null, 3);
		}
	}
}
#endif