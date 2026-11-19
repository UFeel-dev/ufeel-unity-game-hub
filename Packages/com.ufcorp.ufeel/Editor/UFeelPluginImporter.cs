using System;
using System.Linq;
using UFeel;
using UnityEditor;

internal sealed class UFeelPluginImporter : AssetPostprocessor
{
    private const string NativeRoot = "Packages/" + UFeelPackagePaths.PackageName + "/" + UFeelPackagePaths.NativeFolder + "/";
    private const string Wrapper = NativeRoot + "libufeel_wrapper.so";

    [MenuItem("Tools/UFeel/Configure Native Plugins")]
    internal static void ConfigureNativePlugins()
    {
        foreach (PluginImporter plugin in PluginImporter.GetAllImporters())
        {
            if (!plugin.assetPath.StartsWith(NativeRoot, StringComparison.Ordinal))
                continue;

            bool isWrapper = string.Equals(plugin.assetPath, Wrapper, StringComparison.Ordinal);
            if (IsConfigured(plugin, isWrapper))
                continue;

            plugin.SetCompatibleWithAnyPlatform(false);
            plugin.SetCompatibleWithEditor(isWrapper);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, isWrapper);

            if (isWrapper)
            {
                plugin.SetEditorData("OS", "Linux");
                plugin.SetEditorData("CPU", "x86_64");
                plugin.SetPlatformData(BuildTarget.StandaloneLinux64, "CPU", "x86_64");
            }

            plugin.SaveAndReimport();
        }
    }

    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (imported.Any(path => path.StartsWith(NativeRoot, StringComparison.Ordinal)))
            EditorApplication.delayCall += ConfigureNativePlugins;
    }

    private static bool IsConfigured(PluginImporter plugin, bool isWrapper)
    {
        return !plugin.GetCompatibleWithAnyPlatform() &&
            plugin.GetCompatibleWithEditor() == isWrapper &&
            plugin.GetCompatibleWithPlatform(BuildTarget.StandaloneLinux64) == isWrapper;
    }
}
