#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// CI ve yerelde aynı şekilde build almak için basit build method'ları.
/// game-ci/unity-builder@v4 'buildMethod' ile bu statik metodları çağırır.
public static class BuildScript
{
    // Çıkış exe yolu; workflow paketleme adımların "build/StandaloneWindows64" klasörünü zipliyor.
    private const string OutWin64 = "build/StandaloneWindows64/SpaceTrader.exe";

    /// Scenes In Build'te işaretli sahneleri getirir; yoksa exception atar (CI kırmızıya düşer).
    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new System.Exception(
                "No scenes in Build Settings. Add Core/Zephyra and commit ProjectSettings/EditorBuildSettings.asset");

        return scenes;
    }

    /// Geliştirme build'i (Debug/Profiler açık)
    public static void BuildWindowsDevelopment()
    {
        var options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = OutWin64,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
                   | BuildOptions.AllowDebugging
                   | BuildOptions.ConnectWithProfiler
        };

        DoBuild(options);
    }

    /// Yayın build'i (optimize, debug kapalı)
    public static void BuildWindowsRelease()
    {
        var options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = OutWin64,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        DoBuild(options);
    }

    /// Ortak build yürütücü; rapora göre hata fırlatır (CI loglarında görünür).
    private static void DoBuild(BuildPlayerOptions options)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(options.locationPathName));

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception(
                $"Build failed: {summary.result} | errors={summary.totalErrors}, warnings={summary.totalWarnings}");
        }

        UnityEngine.Debug.Log(
            $"Build succeeded: {summary.outputPath} | size={(summary.totalSize / (1024f * 1024f)):0.0} MB");
    }
}
#endif
