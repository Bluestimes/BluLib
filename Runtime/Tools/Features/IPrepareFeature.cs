using System;
using System.Collections.Generic;
using System.Linq;
using BluLib.Internal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BluLib
{
	/// <summary>
	/// Prepare() called on every MonoBehaviour by IPrepareFeature class. If Prepare() returns true, parent scene will be marked dirty 
	/// </summary>
	public interface IPrepare
	{
		bool Prepare();
	}
}

#if UNITY_EDITOR
namespace BluLib.EditorTools
{
    [InitializeOnLoad]
	public class IPrepareFeature
	{
		public static Action OnPrepareBefore;
		public static Action OnPrepare;
		public static Action OnPrepareAfter;

		static IPrepareFeature()
		{
			BluEditorEvents.BeforePlaymode += PrepareOnPlay;
		}

		private static void PrepareOnPlay()
		{
			OnPrepareBefore?.Invoke();
			OnPrepare?.Invoke();
			OnPrepareAfter?.Invoke();
			
			if (BluBoxSettings.PrepareOnPlaymode) RunIPrepare();
		}
		
		/// <summary>
		/// Calls Prepare() on any MonoBehaviour with IPrepare interface. If Prepare() returns true, parent scene will be marked dirty
		/// </summary>
		public static void RunIPrepare()
		{
            BluExtensions.ComponentOfInterface<IPrepare>[] toPrepare = BluExtensions.FindObjectsOfInterfaceAsComponents<IPrepare>();

			HashSet<Scene> modifiedScenes = null;
			foreach (BluExtensions.ComponentOfInterface<IPrepare> prepare in toPrepare)
			{
				bool changed = prepare.Interface.Prepare();

				if (changed && prepare.Component != null)
				{
					if (modifiedScenes == null) modifiedScenes = new();
					modifiedScenes.Add(prepare.Component.gameObject.scene);

					EditorUtility.SetDirty(prepare.Component);
					PrefabUtility.RecordPrefabInstancePropertyModifications(prepare.Component);
					Debug.Log(prepare.Component.name + "." + prepare.Component.GetType().Name + ": Changed on Prepare", prepare.Component);
				}
			}

			if (modifiedScenes != null) EditorSceneManager.SaveScenes(modifiedScenes.ToArray());
		}
	}
}
#endif