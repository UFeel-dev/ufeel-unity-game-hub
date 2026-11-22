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

        /// <summary>Gets the absolute path to the package root.</summary>
        /// <remarks>Editor only.</remarks>
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
