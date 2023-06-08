#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BluLib.Internal
{
    public static class BluBoxUtilities
    {
        private static readonly string MyBoxPackageInfoURL = "https://raw.githubusercontent.com/Deadcows/BluLib/master/package.json";
        private static readonly string MyBoxPackageTag = "com.domybest.mybox";


        public static async void GetMyBoxLatestVersionAsync(Action<BluBoxVersion> onVersionRetrieved)
        {
            try
            {
                using (HttpClient client = new())
                {
                    string packageJson = await client.GetStringAsync(MyBoxPackageInfoURL);

                    string versionRaw = RetrievePackageVersionOutOfJson(packageJson);
                    if (versionRaw == null)
                    {
                        Debug.LogWarning("BluLib was unable to parse package.json :(");
                        return;
                    }

                    BluBoxVersion version = new(versionRaw);
                    if (onVersionRetrieved != null) onVersionRetrieved(version);
                }
            }
            catch (HttpRequestException)
            {
                //It's probably some internet connection issue at this point.
            }
        }

        public static BluBoxVersion GetMyBoxInstalledVersion()
        {
            string packageJsonPath = PackageJsonPath;
            if (packageJsonPath == null)
            {
                Debug.LogWarning("BluLib is unable to check installed version :(");
                return null;
            }

            string packageJsonContents = File.ReadAllText(packageJsonPath);
            string versionRaw = RetrievePackageVersionOutOfJson(packageJsonContents);
            if (versionRaw == null)
            {
                Debug.LogWarning("BluLib was unable to parse package.json :(");
                return null;
            }

            BluBoxVersion version = new(versionRaw);
            return version;
        }

        private static string RetrievePackageVersionOutOfJson(string json)
        {
            string versionLine = json.Split('\r', '\n').SingleOrDefault(l => l.Contains("version"));
            if (versionLine == null) return null;

            MatchCollection matches = Regex.Matches(versionLine, "\"(.*?)\"");
            if (matches.Count <= 1 || matches[1].Value.IsNullOrEmpty()) return null;

            return matches[1].Value.Trim('"');
        }
        

        #region Is Installed Via UPM
        
        public static bool InstalledViaUPM
        {
            get
            {
                if (_installedViaUPMChecked) return _installedViaUPM;

                if (ManifestJsonPath == null)
                {
                    Debug.LogWarning("BluLib is unable to find manifest.json file :(");
                    return false;
                }

                string[] manifest = File.ReadAllLines(ManifestJsonPath);
                _installedViaUPM = manifest.Any(l => l.Contains(MyBoxPackageTag));
                _installedViaUPMChecked = true;
                return _installedViaUPM;
            }
        }

        private static bool _installedViaUPM;
        private static bool _installedViaUPMChecked;

        #endregion
        

        #region Package Json Path

        private static string PackageJsonPath
        {
            get
            {
                if (_packageJsonPathChecked) return _packageJsonPath;

                DirectoryInfo myBoxDirectory = BluBoxInternalPath.MyBoxDirectory;
                if (myBoxDirectory == null)
                {
                    Debug.LogWarning("BluLib is unable to find the path of the package :(");
                    _packageJsonPathChecked = true;
                    return null;
                }

                FileInfo packageJson = myBoxDirectory.GetFiles().SingleOrDefault(f => f.Name == "package.json");
                if (packageJson == null)
                {
                    Debug.LogWarning("BluLib is unable to find package.json file :(");
                    _packageJsonPathChecked = true;
                    return null;
                }

                _packageJsonPath = packageJson.FullName;
                _packageJsonPathChecked = true;
                return _packageJsonPath;
            }
        }

        private static string _packageJsonPath;
        private static bool _packageJsonPathChecked;

        #endregion


        #region Manifest JSON Path

        private static string ManifestJsonPath
        {
            get
            {
                if (_manifestJsonPathChecked) return _manifestJsonPath;

                string packageDir = Application.dataPath.Replace("Assets", "Packages");
                _manifestJsonPath = Directory.GetFiles(packageDir).SingleOrDefault(f => f.EndsWith("manifest.json"));
                _manifestJsonPathChecked = true;
                return _manifestJsonPath;
            }
        }

        private static string _manifestJsonPath;
        private static bool _manifestJsonPathChecked;

        #endregion
    }
}

#endif