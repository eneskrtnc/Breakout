#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    [MenuItem("Build/Windows Development")]
    public static void BuildWindowsDevelopment() => BuildWindows(true);

    public static void BuildWindows(bool development)
    {
        var scenes = EditorBuildSettings.scenes;
        var scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++) scenePaths[i] = scenes[i].path;

        var outDir = "build/StandaloneWindows64";
        Directory.CreateDirectory(outDir);

        var opts = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = Path.Combine(outDir, "SpaceTrader.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = development
                ? BuildOptions.Development | BuildOptions.AllowDebugging
                : BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Build failed: {report.summary.result} | Errors: {report.summary.totalErrors}");
            throw new System.Exception("Build failed");
        }
        Debug.Log($"Build ok. Size: {report.summary.totalSize / (1024 * 1024)} MB");
    }
}
#endif