using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Unity CI 构建脚本 - 供 GitHub Actions 调用 (game-ci/unity-builder)
/// 将所有素材与脚本打包为可安装的 Android APK。
/// 所有 Resources 下的 643 个资源文件 (GLB/PNG/OGG 等) 均会被打包进 APK。
/// </summary>
public static class CIBuildScript
{
    /// <summary>
    /// 构建 Android APK (被 workflow 调用)
    /// </summary>
    public static void BuildAndroid()
    {
        Debug.Log("=== CI Build: 开始构建 Android APK ===");

        // 切换到 Android 平台
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android, BuildTarget.Android);

        // 确保输出目录存在
        string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "CIBuild");
        Directory.CreateDirectory(buildDir);

        // 收集所有激活的场景
        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Debug.LogWarning("未配置激活场景，尝试创建默认场景");
            // 创建一个包含 Board 的默认场景（fallback）
            // 此时会使用当前打开的场景
            scenes = new string[] { EditorBuildSettings.scenes.Length > 0
                ? EditorBuildSettings.scenes[0].path
                : "Assets/Scenes/Main.unity" };
        }

        Debug.Log($"待构建场景: {string.Join(", ", scenes)}");
        Debug.Log($"资源文件数: {Directory.GetFiles("Assets/Resources", "*", SearchOption.AllDirectories).Length}");

        // 构建选项
        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(buildDir, "ChineseChess3D.apk"),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✓ 构建成功: {summary.totalSize} bytes -> {opts.locationPathName}");
        }
        else
        {
            Debug.LogError($"✗ 构建失败: {summary.result}");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// 获取已启用的场景列表
    /// </summary>
    private static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s.enabled) scenes.Add(s.path);
        }
        return scenes.ToArray();
    }
}
