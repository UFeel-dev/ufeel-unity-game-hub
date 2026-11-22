#nullable enable

using System.IO;
#if !UNITY_EDITOR
using UnityEngine;
#endif

namespace UFeel
{
    public static class UFeelPackagePaths
    {
        public const string PackageName = "com.ufcorp.ufeel";

        public const string NativeFolder = "Runtime/Plugins/Linux-x86_64";

        /// <summary>Relative to StreamingAssets in players.</summary>
        public const string PlayerFolder = "UFeel";

        /// <summary>Editor only.</summary>
        public static string PackageRoot => Path.GetFullPath("Packages/" + PackageName);

        public static string NativeRoot
        {
            get =>
#if UNITY_EDITOR
                Path.Combine(PackageRoot, NativeFolder);
#else
                Path.Combine(Application.streamingAssetsPath, PlayerFolder);
#endif

        }
    }
}
