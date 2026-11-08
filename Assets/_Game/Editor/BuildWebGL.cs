using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildWebGL
{
    public static void Build()
    {
        string[] args = Environment.GetCommandLineArgs();
        int outputOption = Array.IndexOf(args, "-seedWebglOutput");
        if (outputOption < 0 || outputOption + 1 >= args.Length)
            throw new ArgumentException("Pass -seedWebglOutput followed by the output folder.");

        string output = Path.GetFullPath(args[outputOption + 1]);
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        if (scenes.Length != 3)
            throw new InvalidOperationException("Seed WebGL build requires all three game scenes.");

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Seed WebGL build failed: " + report.summary.result);

        Debug.Log("Seed WebGL build complete: " + output);
    }
}
