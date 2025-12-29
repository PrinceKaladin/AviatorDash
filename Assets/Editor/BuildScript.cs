using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;

public class BuildScript
{
    private static string[] scenes = {
        "Assets/Scenes/MainMenuScene.unity",
        "Assets/Scenes/GamePlayScene.unity"
    };

    private static void ConfigureKeystore()
    {
        string keystoreBase64 = "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";
        string keystorePass = "R3d$trike!92";
        string keyAlias = "aviatordash";
        string keyPass = "R3d$trike!92";

        if (!string.IsNullOrEmpty(keystoreBase64))
        {
            string tempKeystorePath = Path.Combine(Path.GetTempPath(), "TempKeystore.jks");
            File.WriteAllBytes(tempKeystorePath, Convert.FromBase64String(keystoreBase64));

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = tempKeystorePath;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = keyAlias;
            PlayerSettings.Android.keyaliasPass = keyPass;

            Debug.Log("Android signing configured from Base64 keystore.");
        }
        else
        {
            Debug.LogWarning("Keystore Base64 not set. Build will be unsigned.");
            PlayerSettings.Android.useCustomKeystore = false;
        }
    }

    public static void PerformBuildAAB()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        ConfigureKeystore();

        EditorUserBuildSettings.buildAppBundle = true;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "AviatorDash.aab",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log("✅ AAB build succeeded!");
        else
            Debug.LogError("❌ AAB build failed!");
    }

    public static void PerformBuildAPK()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        ConfigureKeystore();

        EditorUserBuildSettings.buildAppBundle = false;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "AviatorDash.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log("✅ APK build succeeded!");
        else
            Debug.LogError("❌ APK build failed!");
    }
}