using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Reduces WebGL linker memory usage to avoid wasm-ld out-of-memory failures on 16 GB machines.
/// </summary>
public sealed class WebGLBuildMemoryFix : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.BuildTimes;

        var webglTarget = NamedBuildTarget.WebGL;
        PlayerSettings.SetIl2CppCodeGeneration(webglTarget, Il2CppCodeGeneration.OptimizeSpeed);

        if ((report.summary.options & BuildOptions.Development) != 0)
        {
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        }

        Debug.Log(
            "[WebGLBuildMemoryFix] Applied low-memory WebGL settings: " +
            "codeOptimization=BuildTimes, il2CppCodeGeneration=OptimizeSpeed. " +
            "Close other apps before building; 16 GB RAM machines need several GB free for wasm-ld."
        );
    }
}
