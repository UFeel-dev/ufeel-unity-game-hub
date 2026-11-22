using System.IO;
using System.Linq;
using UFeel;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

internal sealed class UFeelBuildPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.StandaloneLinux64)
            return;

        string source = Path.Combine(UFeelPackagePaths.PackageRoot, UFeelPackagePaths.NativeFolder);
        string output = report.summary.outputPath;
        string dataDirectory = Path.Combine(
            Path.GetDirectoryName(output) ?? string.Empty,
            Path.GetFileNameWithoutExtension(output) + "_Data");

        string wrapper = Directory
            .GetFiles(Path.Combine(dataDirectory, "Plugins"), "libufeel_wrapper.so", SearchOption.AllDirectories)
            .FirstOrDefault()
            ?? throw new BuildFailedException("libufeel_wrapper.so is missing from the player plugins.");

        CopyDirectory(
            Path.Combine(source, "dependencies"),
            Path.Combine(Path.GetDirectoryName(wrapper), "dependencies"));
        CopyDirectory(
            Path.Combine(source, "models"),
            Path.Combine(dataDirectory, "StreamingAssets", UFeelPackagePaths.PlayerFolder, "models"));
    }

    private static void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(source))
            return;

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".meta", System.StringComparison.Ordinal))
                continue;

            string target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.Copy(file, target, true);
        }
    }
}
