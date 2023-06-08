#if UNITY_EDITOR
using System;

namespace BluLib.Internal
{
    [Serializable]
    public class BluBoxVersion
    {
        public string Major;
        public string Minor;
        public string Patch;

        public string AsSting;

        /// <param name="version">NUM.NUM.NUM format</param>
        public BluBoxVersion(string version)
        {
            AsSting = version;
            string[] v = version.Split('.');
            Major = v[0];
            Minor = v[1];
            Patch = v[2];
        }

        /// <summary>
        /// Major & Minor versions match, skip patch releases
        /// </summary>
        public bool BaseVersionMatch(BluBoxVersion version)
        {
            return Major == version.Major && Minor == version.Minor;
        }

        public bool VersionsMatch(BluBoxVersion version)
        {
            return BaseVersionMatch(version) && Patch == version.Patch;
        }
    }
}
#endif