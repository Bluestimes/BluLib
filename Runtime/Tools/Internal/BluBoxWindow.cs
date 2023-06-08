#if UNITY_EDITOR
using BluLib.EditorTools;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace BluLib.Internal
{
	[InitializeOnLoad]
	public class BluBoxWindow : EditorWindow
	{
		private static BluBoxVersion _installedVersion;
		private static BluBoxVersion _latestVersion;

		private static EditorWindow _windowInstance;

		private GUIStyle _titleStyle;
		private GUIStyle _buttonStyle;

		private AddRequest _updateRequest;


		static BluBoxWindow()
		{
			if (BluBoxSettings.CheckForUpdates) BluEditorEvents.OnEditorStarts += CheckForUpdates;
		}

		private static void CheckForUpdates()
		{
			BluEditorEvents.OnEditorStarts -= CheckForUpdates;
			BluBoxUtilities.GetMyBoxLatestVersionAsync(version =>
			{
				_installedVersion = BluBoxUtilities.GetMyBoxInstalledVersion();
				_latestVersion = version;
				if (!_installedVersion.VersionsMatch(_latestVersion))
				{
					string versions = "Installed version: " + _installedVersion.AsSting + ". Latest version: " + _latestVersion.AsSting;
					string message = "It's time to update BluLib :)! Use \"Tools/BluLib/Update BluLib\". " + versions;
					WarningsPool.Log(message);
				}
			});
		}


		[MenuItem("Tools/BluLib/BluLib Window", priority = 1)]
		private static void MyBoxWindowMenuItem()
		{
			_windowInstance = GetWindow<BluBoxWindow>();
			_windowInstance.titleContent = new("BluLib");
			_windowInstance.minSize = new(590, 520);
			_windowInstance.maxSize = new(590, 520);
		}

		private void OnEnable()
		{
			_windowInstance = this;

			_installedVersion = BluBoxUtilities.GetMyBoxInstalledVersion();
			BluBoxUtilities.GetMyBoxLatestVersionAsync(version =>
			{
				_latestVersion = version;
				if (_windowInstance != null) _windowInstance.Repaint();
			});
		}


		private void OnGUI()
		{
			if (_titleStyle == null)
			{
				_titleStyle = new(EditorStyles.boldLabel);
				_titleStyle.fontSize = 42;
				_titleStyle.fontStyle = FontStyle.BoldAndItalic;
				_titleStyle.alignment = TextAnchor.MiddleCenter;
			}

			if (_buttonStyle == null)
			{
				_buttonStyle = new(BluGUI.HelpBoxStyle);
				_buttonStyle.hover.textColor = BluGUI.Colors.Blue;
			}

			GUILayoutOption buttonWidth = GUILayout.Width(120);
			GUILayoutOption buttonHeight = GUILayout.Height(30);
			int leftOffset = 20;


			wantsMouseMove = true;
			if (Event.current.type == EventType.MouseMove) Repaint();


			//buttonStyle.hover.background = buttonStyle.active.background.WithSolidColor(Color.red);

			EditorGUILayout.Space();


			EditorGUILayout.LabelField("BluLib", _titleStyle, GUILayout.Height(60));

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();

				if (GUILayout.Button("  Github Page ↗", _buttonStyle, buttonWidth, buttonHeight))
					Application.OpenURL("https://github.com/Deadcows/BluLib");

				if (GUILayout.Button("  Attributes ↗", _buttonStyle, buttonWidth, buttonHeight))
					Application.OpenURL("https://github.com/Deadcows/BluLib/wiki/Attributes");

				if (GUILayout.Button("  Extensions ↗", _buttonStyle, buttonWidth, buttonHeight))
					Application.OpenURL("https://github.com/Deadcows/BluLib/tree/master/Extensions");

				if (GUILayout.Button("  Tools, Features ↗", _buttonStyle, buttonWidth, buttonHeight))
					Application.OpenURL("https://github.com/Deadcows/BluLib/wiki/Tools-and-Features");

				GUILayout.FlexibleSpace();
			}

			BluGUI.DrawLine(Color.white, true);

			EditorGUILayout.LabelField("BluLib Settings", new GUIStyle(EditorStyles.centeredGreyMiniLabel));

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				using (new EditorGUILayout.VerticalScope())
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.Space(leftOffset);
						BluBoxSettings.CheckForUpdates = EditorGUILayout.Toggle("Check for Updates: ", BluBoxSettings.CheckForUpdates);
						GUILayout.FlexibleSpace();
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						GUIContent label = new("AutoSave on Play: ", "Save changes in opened scenes before Playmode. " +
                                                                     "\nUnity crasher from time to time you know...");
						EditorGUILayout.Space(leftOffset);
						BluBoxSettings.AutoSaveEnabled = EditorGUILayout.Toggle(label, BluBoxSettings.AutoSaveEnabled);
						GUILayout.FlexibleSpace();
					}

					using (new EditorGUILayout.HorizontalScope())
					{
						GUIContent label = new("Clean Empty Folders: ", "Delete empty folders in project on Save. " +
                                                                        "\nIt handles VCS issue with .meta files for empty folders");
						EditorGUILayout.Space(leftOffset);
						BluBoxSettings.CleanEmptyDirectoriesFeature = EditorGUILayout.Toggle(label, BluBoxSettings.CleanEmptyDirectoriesFeature);
						GUILayout.FlexibleSpace();
					}
				}

				EditorGUILayout.Space(80);
				using (new EditorGUILayout.VerticalScope())
				{
					EditorGUILayout.LabelField("Performance settings", EditorStyles.miniLabel);
					
					using (new EditorGUILayout.HorizontalScope())
					{
						GUIContent label = new("Prepare on Playmode: ", "Allows to use IPrepare interface with Prepare() method called automatically." +
                                                                        "\nSlightly increases project Save time.");
						BluBoxSettings.PrepareOnPlaymode = EditorGUILayout.Toggle(label, BluBoxSettings.PrepareOnPlaymode);
						if (GUILayout.Button(BluGUI.EditorIcons.Help, EditorStyles.label, GUILayout.Height(18)))
							Application.OpenURL("https://github.com/Deadcows/BluLib/wiki/Tools-and-Features#iprepare");
						GUILayout.FlexibleSpace();
					}
					
					using (new EditorGUILayout.HorizontalScope())
					{
						GUIContent label = new("SO processing: ", "Allows [AutoProperty] and [MustBeAssigned] Attributes to work with Scriptable Objects." +
                                                                  "\nMight increase project Save time for a few seconds.");
						BluBoxSettings.EnableSOCheck = EditorGUILayout.Toggle(label, BluBoxSettings.EnableSOCheck);
						GUILayout.FlexibleSpace();
					}
				}
				GUILayout.FlexibleSpace();
			}
			
			


			BluGUI.DrawLine(Color.white, true);

			using (new EditorGUILayout.HorizontalScope())
			{
				string current = _installedVersion == null ? "..." : _installedVersion.AsSting;
				string latest = _latestVersion == null ? "..." : _latestVersion.AsSting;
				string installationType = BluBoxUtilities.InstalledViaUPM ? "UPM" : "(not UPM)";
				GUIStyle versionStyle = new(EditorStyles.miniBoldLabel);
				versionStyle.alignment = TextAnchor.MiddleCenter;
				EditorGUILayout.LabelField($@"current: {current} {installationType}. latest: {latest}", versionStyle);
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = _updateRequest == null || _updateRequest.IsCompleted;
				string updateOrInstall = BluBoxUtilities.InstalledViaUPM ? "Update" : "Install";
				if (GUILayout.Button(updateOrInstall + " UPM version", _buttonStyle, buttonWidth, buttonHeight))
				{
					if (BluBoxUtilities.InstalledViaUPM) AddPackage();
					else
					{
						if (EditorUtility.DisplayDialog(
							"Warning before installation",
							"When UPM version will be imported you should delete current installation of BluLib",
							"Ok, Install UPM version!", "Nah, keep it as it is")) AddPackage();
					}

					void AddPackage()
					{
						_updateRequest = Client.Add("https://github.com/Deadcows/BluLib.git");
					}
				}

				GUI.enabled = true;

				if (GUILayout.Button("  How to Update ↗", _buttonStyle, buttonWidth, buttonHeight))
					Application.OpenURL("https://github.com/Deadcows/BluLib/wiki/Installation");

				if (GUILayout.Button("  Releases ↗", _buttonStyle, buttonWidth, buttonHeight))
					Application.OpenURL("https://github.com/Deadcows/BluLib/releases");

				if (GUILayout.Button("  Changelog ↗", _buttonStyle, buttonWidth, buttonHeight))
					Application.OpenURL("https://github.com/Deadcows/BluLib/blob/master/CHANGELOG.md");

				GUILayout.FlexibleSpace();
			}

			BluGUI.DrawLine(Color.white, true);

			EditorGUILayout.LabelField("MyGUI.Colors References", new GUIStyle(EditorStyles.centeredGreyMiniLabel));
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				DrawColors();
				GUILayout.FlexibleSpace();
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("MyGUI.EditorIcons References + with black color tint", new GUIStyle(EditorStyles.centeredGreyMiniLabel));
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				DrawIcons();
				GUILayout.FlexibleSpace();
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				Color c = GUI.contentColor;
				GUI.contentColor = Color.black;
				DrawIcons();
				GUI.contentColor = c;
				GUILayout.FlexibleSpace();
			}

			EditorGUILayout.Space(40);
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				GUIStyle sponsorButtonStyle = new(EditorStyles.centeredGreyMiniLabel);
				sponsorButtonStyle.fontSize *= 2;
				sponsorButtonStyle.fontStyle = FontStyle.Italic;
				if (GUILayout.Button("buy me a coffee :)", sponsorButtonStyle, GUILayout.Height(32)))
					Application.OpenURL("https://www.buymeacoffee.com/andrewrumak");
				GUILayout.FlexibleSpace();
			}
		}

		private void DrawColors()
		{
			int width = 24;
			int height = (int) EditorGUIUtility.singleLineHeight;

			GUIContent content = new("", "MyGUI.Colors.Red");
			EditorGUILayout.LabelField(content, GUILayout.Width(width));
			BluGUI.DrawBackgroundBox(BluGUI.Colors.Red, height);

			content = new("", "MyGUI.Colors.Green");
			EditorGUILayout.LabelField(content, GUILayout.Width(width));
			BluGUI.DrawBackgroundBox(BluGUI.Colors.Green, height);

			content = new("", "MyGUI.Colors.Blue");
			EditorGUILayout.LabelField(content, GUILayout.Width(width));
			BluGUI.DrawBackgroundBox(BluGUI.Colors.Blue, height);

			content = new("", "MyGUI.Colors.Gray");
			EditorGUILayout.LabelField(content, GUILayout.Width(width));
			BluGUI.DrawBackgroundBox(BluGUI.Colors.Gray, height);

			content = new("", "MyGUI.Colors.Yellow");
			EditorGUILayout.LabelField(content, GUILayout.Width(width));
			BluGUI.DrawBackgroundBox(BluGUI.Colors.Yellow, height);

			content = new("", "MyGUI.Colors.Brown");
			EditorGUILayout.LabelField(content, GUILayout.Width(width));
			BluGUI.DrawBackgroundBox(BluGUI.Colors.Brown, height);
		}

		private void DrawIcons()
		{
			int width = 24;
			GUIContent content = new(BluGUI.EditorIcons.Plus);
			content.tooltip = "MyGUI.EditorIcons.Plus";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Minus);
			content.tooltip = "MyGUI.EditorIcons.Minus";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Refresh);
			content.tooltip = "MyGUI.EditorIcons.Refresh";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.ConsoleInfo);
			content.tooltip = "MyGUI.EditorIcons.ConsoleInfo";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.ConsoleWarning);
			content.tooltip = "MyGUI.EditorIcons.ConsoleWarning";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.ConsoleError);
			content.tooltip = "MyGUI.EditorIcons.ConsoleError";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Check);
			content.tooltip = "MyGUI.EditorIcons.Check";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Cross);
			content.tooltip = "MyGUI.EditorIcons.Cross";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Dropdown);
			content.tooltip = "MyGUI.EditorIcons.Dropdown";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.EyeOn);
			content.tooltip = "MyGUI.EditorIcons.EyeOn";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.EyeOff);
			content.tooltip = "MyGUI.EditorIcons.EyeOff";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Zoom);
			content.tooltip = "MyGUI.EditorIcons.Zoom";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Help);
			content.tooltip = "MyGUI.EditorIcons.Help";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Favourite);
			content.tooltip = "MyGUI.EditorIcons.Favourite";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Label);
			content.tooltip = "MyGUI.EditorIcons.Label";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Settings);
			content.tooltip = "MyGUI.EditorIcons.Settings";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.SettingsPopup);
			content.tooltip = "MyGUI.EditorIcons.SettingsPopup";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.SettingsMixer);
			content.tooltip = "MyGUI.EditorIcons.SettingsMixer";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.Circle);
			content.tooltip = "MyGUI.EditorIcons.Circle";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.CircleYellow);
			content.tooltip = "MyGUI.EditorIcons.CircleYellow";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.CircleDotted);
			content.tooltip = "MyGUI.EditorIcons.CircleDotted";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));

			content = new(BluGUI.EditorIcons.CircleRed);
			content.tooltip = "MyGUI.EditorIcons.CircleRed";
			EditorGUILayout.LabelField(content, GUILayout.Width(width));
		}
	}
}
#endif