#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class BuildScript
{
    [MenuItem("Build/Windows Development")]
    public static void BuildWindowsDevelopment() => BuildWindows(true);

    // CI’nın çağıracağı RELEASE entrypoint
    public static void BuildWindowsRelease() => BuildWindows(false);

    public static void BuildWindows(bool development)
    {
        var scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenePaths.Length == 0)
            throw new Exception("Build Settings > Scenes In Build içine en az bir sahne eklemelisin.");

        // Çıkış klasörü (ENV ile override edilebilir)
        var outDir = Environment.GetEnvironmentVariable("UNITY_BUILD_OUTPUT");
        if (string.IsNullOrEmpty(outDir))
            outDir = development ? "build/StandaloneWindows64Dev" : "build/StandaloneWindows64";
        Directory.CreateDirectory(outDir);

        var opts = development
            ? BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler
            : BuildOptions.None;

        var bpo = new BuildPlayerOptions
        {
            scenes = scenePaths,
            target = BuildTarget.StandaloneWindows64,
            locationPathName = Path.Combine(outDir, "SpaceTrader.exe"),
            options = opts
        };

        var sw = Stopwatch.StartNew();
        var report = BuildPipeline.BuildPlayer(bpo);
        sw.Stop();

        var ok = report.summary.result == BuildResult.Succeeded;
        var sizeMb = report.summary.totalSize / (1024f * 1024f);

        // GitHub Actions Step Summary (lokalde yoksa Console'a yazar)
        var summaryPath = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        var text =
            "## Unity Build\n" +
            $"- Result: {(ok ? "Succeeded ✅" : "Failed ❌")}\n" +
            $"- Mode: {(development ? "Development" : "Release")}\n" +
            $"- Duration: {sw.Elapsed:mm\\:ss}\n" +
            $"- Artifact size: {sizeMb:0.00} MB\n" +
            $"- Output: `{bpo.locationPathName.Replace("\\", "/")}`\n";

        if (!string.IsNullOrEmpty(summaryPath))
                    File.AppendAllText(summaryPath, text);
        else
            Debug.Log(text);

        if (!ok)
            throw new Exception($"Build failed: {report.summary.result} | Errors: {report.summary.totalErrors}");
    }
}
#endif
