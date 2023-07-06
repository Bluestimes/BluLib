using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace BluLib.Internal
{
    [InitializeOnLoad]
	public class RequireLayerOtRagAttributeHandler
	{
		static RequireLayerOtRagAttributeHandler()
		{
			EditorApplication.playModeStateChanged += AutoSaveWhenPlaymodeStarts;
		}

		private static void AutoSaveWhenPlaymodeStarts(PlayModeStateChange obj)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			{
				Component[] components = Object.FindObjectsOfType<Component>();
				foreach (Component component in components)
				{
					foreach (object attribute in component.GetType().GetCustomAttributes(true))
					{
						RequireLayerAttribute layerAttribute = attribute as RequireLayerAttribute;
						if (layerAttribute != null)
						{
							int requiredLayer = layerAttribute.LayerName != null ? 
								LayerMask.NameToLayer(layerAttribute.LayerName) : 
								layerAttribute.LayerIndex;
							if (component.gameObject.layer == requiredLayer) continue;

							Debug.LogWarning("Layer of " + component.name + " changed by RequireLayerAttribute to " + layerAttribute.LayerName);
							component.gameObject.layer = requiredLayer;
							EditorUtility.SetDirty(component);
							
							continue;
						}

						RequireTagAttribute tagAttribute = attribute as RequireTagAttribute;
						if (tagAttribute != null)
						{
							if (component.CompareTag(tagAttribute.Tag)) continue;

							Debug.LogWarning("Tag of " + component.name + " changed by RequireTagAttribute to " + tagAttribute.Tag);
							component.gameObject.tag = tagAttribute.Tag;
							EditorUtility.SetDirty(component);
						}
					}
				}
			}
		}
	}
}
#endif
