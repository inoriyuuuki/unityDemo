using UnityEditor;
using UnityEngine;

namespace FMBG.EditorTools
{
    /// <summary>一键构建 macOS 可执行版。</summary>
    public static class GameBuild
    {
        [MenuItem("Game/Tools/Build macOS")]
        public static void BuildMac()
        {
            string[] scenes = { "Assets/Game/Scenes/Main.unity" };
            string outDir = "Builds/EnemyAIDemo_mac";

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outDir,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("[GameBuild] 构建成功: " + outDir);
            }
            else
            {
                Debug.LogError("[GameBuild] 构建失败: " + report.summary.result);
                foreach (var err in report.steps)
                {
                    Debug.LogError("[GameBuild] step: " + err.name);
                }
            }
        }
    }
}
