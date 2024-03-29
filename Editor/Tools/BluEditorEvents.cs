#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BluLib.EditorTools
{
	[InitializeOnLoad]
	public class BluEditorEvents : AssetModificationProcessor, IPreprocessBuildWithReport
	{
		/// <summary>
		/// Occurs on Scenes/Assets Save
		/// </summary>
		public static event Action OnSave;

		/// <summary>
		/// Occurs on first frame in Playmode
		/// </summary>
		public static event Action OnFirstFrame;

		public static event Action BeforePlaymode;

		public static event Action BeforeBuild;

		/// <summary>
		/// Occurs on second frame after editor starts
		/// (to notify all scripts subscribed with [InitializeOnLoad])
		/// </summary>
		public static event Action OnEditorStarts;

		public static event Action<Event> OnEditorInput;


		static BluEditorEvents()
		{
			EditorApplication.update += CheckOnceOnEditorStart;
			EditorApplication.update += CheckOnceOnPlaymode;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			RegisterRawInputHandler();


			void RegisterRawInputHandler()
			{
				FieldInfo globalEventHandler = typeof(EditorApplication).GetField("globalEventHandler",
					BindingFlags.Static | BindingFlags.NonPublic);
				if (globalEventHandler == null) return;
				EditorApplication.CallbackFunction callback = (EditorApplication.CallbackFunction)globalEventHandler.GetValue(null);
				callback += RawInputHandler;
				globalEventHandler.SetValue(null, callback);
			}
		}

		/// <summary>
		/// On Editor Save
		/// </summary>
		private static string[] OnWillSaveAssets(string[] paths)
		{
			// Prefab creation enforces SaveAsset and this may cause unwanted dir cleanup
			if (paths.Length == 1 && (paths[0] == null || paths[0].EndsWith(".prefab"))) return paths;

			OnSave?.Invoke();

			return paths;
		}

		private static void CheckOnceOnEditorStart()
		{
			if (!_skipFrameOnEditorStart)
			{
				_skipFrameOnEditorStart = true;
				return;
			}
			
			if (!SessionState.GetBool("EditorInitiated", false))
			{
				SessionState.SetBool("EditorInitiated", true);
				OnEditorStarts?.Invoke();
			}
		}

		private static bool _skipFrameOnEditorStart;


		/// <summary>
		/// On First Frame
		/// </summary>
		private static void CheckOnceOnPlaymode()
		{
			if (Application.isPlaying)
			{
				EditorApplication.update -= CheckOnceOnPlaymode;
				OnFirstFrame?.Invoke();
			}
		}

		/// <summary>
		/// On Before Playmode
		/// </summary>
		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingEditMode && BeforePlaymode != null) BeforePlaymode();
		}

		private static void RawInputHandler()
		{
			Event e = Event.current;
			if (e.type != EventType.KeyDown || e.keyCode == KeyCode.None) return;

			OnEditorInput?.Invoke(e);
		}

		/// <summary>
		/// Before Build
		/// </summary>
		public void OnPreprocessBuild(BuildReport report) => BeforeBuild?.Invoke();

		public int callbackOrder => 0;
	}
}
#endif