#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace BluLib.Internal
{
	[InitializeOnLoad]
	public class AutoSaveFeature
	{
		static AutoSaveFeature()
		{
			EditorApplication.playModeStateChanged += AutoSaveWhenPlaymodeStarts;
		}

		private static void AutoSaveWhenPlaymodeStarts(PlayModeStateChange obj)
		{
			if (!BluBoxSettings.AutoSaveEnabled) return;

			if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					Scene scene = SceneManager.GetSceneAt(i);
					if (scene.isDirty) EditorSceneManager.SaveScene(scene);
				}

				AssetDatabase.SaveAssets();
			}
		}
	}
}
#endif