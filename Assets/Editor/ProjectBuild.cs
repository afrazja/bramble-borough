using System;
using System.IO;
using BrambleBorough;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectBuild
{
    const string Scene = "Assets/Scenes/LanternGrove.unity";
    [MenuItem("Bramble Borough/Prepare playable scene")]
    public static void Prepare()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Camera").AddComponent<Camera>();
        camera.orthographic = true; camera.backgroundColor = GroveGame.Ink;
        camera.clearFlags = CameraClearFlags.SolidColor;
        new GameObject("Lantern Grove - Game").AddComponent<GroveGame>();
        EditorSceneManager.SaveScene(scene, Scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Scene, true) };
        EditorSettings.serializationMode = SerializationMode.ForceText;
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        PlayerSettings.companyName = "Bramble Borough";
        PlayerSettings.productName = "Bramble Borough";
        PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.brambleborough.lanterngrove");
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.initialMemorySize = 128;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        QualitySettings.vSyncCount = 0;
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("BRAMBLE: scene and lightweight settings prepared.");
    }
    static void Require(bool condition, string message) { if (!condition) throw new Exception("FAILED: " + message); }
    [MenuItem("Bramble Borough/Run gameplay checks")]
    public static void Verify()
    {
        var g = new GroveState();
        Require(!g.Place(-1,1) && !g.Place(8,1) && !g.Place(0,3), "invalid placements rejected");
        Require(g.Place(0,1) && g.Coins == 120, "placement deducts exact cost");
        Require(!g.Place(0,2) && g.Coins == 120, "occupied pad cannot charge coins");
        Require(g.Place(1,2) && !g.Place(2,1), "insufficient funds rejected");
        Require(g.StartWave() && !g.StartWave(), "wave cannot start twice");
        var loss = new GroveState();
        for(int wave=0;wave<3 && !loss.Lost;wave++) { loss.StartWave(); for(int t=0;t<12000 && loss.Running;t++) loss.Tick(.05f); }
        Require(loss.Lost && loss.Health==0 && !loss.StartWave() && !loss.Place(0,1), "undefended grove loses and locks gameplay");
        var win = new GroveState();
        Require(win.Place(1,1) && win.Place(3,1) && win.Place(4,1), "starter strategy affordable");
        for(int wave=0;wave<3;wave++)
        {
            Require(win.StartWave(), "next wave starts");
            for(int t=0;t<12000 && win.Running;t++)
            {
                win.Tick(.05f);
                foreach(int pad in new[]{5,0,6,2,7}) if(win.Coins>=60 && win.Towers[pad]==0) win.Place(pad,1);
            }
            Require(!win.Running && !win.Lost, "wave completes within time limit");
        }
        Require(win.Won && win.Kills>0 && !win.StartWave(), "three waves win and lock gameplay");
        float frozen = win.Enemies.Count; win.Tick(10); Require(win.Enemies.Count == frozen, "terminal state stable");
        var reset = new GroveState(); Require(reset.Coins==180 && reset.Health==10 && reset.Wave==0 && reset.Enemies.Count==0, "fresh game resets state");
        Directory.CreateDirectory("Builds");
        File.WriteAllText("Builds/gameplay-checks.txt", "PASS: placement, funds, occupied pads, wave gating, loss, viable three-wave victory, terminal state, fresh reset.\nVictory lives: "+win.Health+"; kills: "+win.Kills);
        Debug.Log("BRAMBLE GAMEPLAY CHECKS PASSED: victory lives="+win.Health+", kills="+win.Kills);
    }
    public static void SetupAndVerify() { Prepare(); Verify(); }
    public static void Windows() { Build(BuildTarget.StandaloneWindows64,"Builds/Windows/BrambleBorough.exe"); }
    public static void Web() { Build(BuildTarget.WebGL,"Builds/Web"); }
    public static void Android() { Build(BuildTarget.Android,"Builds/Android/BrambleBorough.apk"); }
    static void Build(BuildTarget target,string path)
    {
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{Scene}, locationPathName=path, target=target, options=BuildOptions.None });
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Build failed: "+report.summary.result);
        Debug.Log("BRAMBLE BUILD PASSED: "+path);
    }
}
