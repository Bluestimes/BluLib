#if UNITY_EDITOR
using UnityEditor;

namespace BluLib.Internal
{
	public static class BluBoxMenuItems
	{
		private const string Base = "Tools/BluLib/";
		private const string AutoSaveMenuItemKey = Base + "AutoSave on play";
		private const string CleanupEmptyDirectoriesMenuItemKey = Base + "Clear empty directories On Save";
		private const string PrepareMenuItemKey = Base + "Run Prepare on play";
		private const string CheckForUpdatesKey = Base + "Check for updates on start";


		#region AutoSave

		[MenuItem(AutoSaveMenuItemKey, priority = 100)]
		private static void AutoSaveMenuItem()
			=> BluBoxSettings.AutoSaveEnabled = !BluBoxSettings.AutoSaveEnabled;

		[MenuItem(AutoSaveMenuItemKey, true)]
		private static bool AutoSaveMenuItemValidation()
		{
			Menu.SetChecked(AutoSaveMenuItemKey, BluBoxSettings.AutoSaveEnabled);
			return true;
		}

		#endregion


		#region CleanupEmptyDirectories

		[MenuItem(CleanupEmptyDirectoriesMenuItemKey, priority = 100)]
		private static void CleanupEmptyDirectoriesMenuItem()
			=> BluBoxSettings.CleanEmptyDirectoriesFeature = !BluBoxSettings.CleanEmptyDirectoriesFeature;

		[MenuItem(CleanupEmptyDirectoriesMenuItemKey, true)]
		private static bool CleanupEmptyDirectoriesMenuItemValidation()
		{
			Menu.SetChecked(CleanupEmptyDirectoriesMenuItemKey, BluBoxSettings.CleanEmptyDirectoriesFeature);
			return true;
		}

		#endregion


		#region Prepare

		[MenuItem(PrepareMenuItemKey, priority = 100)]
		private static void PrepareMenuItem()
			=> BluBoxSettings.PrepareOnPlaymode = !BluBoxSettings.PrepareOnPlaymode;

		[MenuItem(PrepareMenuItemKey, true)]
		private static bool PrepareMenuItemValidation()
		{
			Menu.SetChecked(PrepareMenuItemKey, BluBoxSettings.PrepareOnPlaymode);
			return true;
		}

		#endregion


		#region Check For Updates

		[MenuItem(CheckForUpdatesKey, priority = 100)]
		private static void CheckForUpdatesMenuItem()
			=> BluBoxSettings.CheckForUpdates = !BluBoxSettings.CheckForUpdates;

		[MenuItem(CheckForUpdatesKey, true)]
		private static bool CheckForUpdatesMenuItemValidation()
		{
			Menu.SetChecked(CheckForUpdatesKey, BluBoxSettings.CheckForUpdates);
			return true;
		}

		#endregion
	}
}
#endif