#if UNITY_EDITOR
using System.IO;
using BluLib.EditorTools;
using UnityEngine;

namespace BluLib.Internal
{
    /// <summary>
    /// SO is needed to determine the path to this script.
    /// Thereby it's used to get relative path to BluLib
    /// </summary>
    public class BluBoxInternalPath : ScriptableObject
    {
        /// <summary>
        /// Absolute path to BluLib folder
        /// </summary>
        public static DirectoryInfo MyBoxDirectory
        {
            get
            {
                if (_directoryChecked) return _myBoxDirectory;
                
                string internalPath = BluEditor.GetScriptAssetPath(Instance);
                DirectoryInfo scriptDirectory = new(internalPath);

                // Script is in BluLib/Tools/Internal so we need to get dir two steps up in hierarchy
                if (scriptDirectory.Parent == null || scriptDirectory.Parent.Parent == null)
                {
                    _directoryChecked = true;
                    return null;
                }

                _myBoxDirectory = scriptDirectory.Parent.Parent;
                _directoryChecked = true;
                return _myBoxDirectory;
            }
        }

        private static DirectoryInfo _myBoxDirectory;
        private static bool _directoryChecked;

        private static BluBoxInternalPath Instance
        {
            get
            {
                if (_instance != null) return _instance;
                return _instance = CreateInstance<BluBoxInternalPath>();
            }
        }

        private static BluBoxInternalPath _instance;
    }
}
#endif