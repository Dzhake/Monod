using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Monod.AssetsModule;
using System;
using System.IO;

namespace Monod.MSBuild;

/// <summary>
/// Used to compile .fx asset files using MGFXC.
/// </summary>
public class CompileEffectsTask : Task
{
    /// <summary>
    /// List of <see cref="File"/> paths, relative to <see cref="PathToContent"/>, of files which should be compiled.
    /// </summary>
    public string? Effects { get; set; }

    /// <summary>
    /// Absolute <see cref="Directory"/> path, where assets should be outputted, before their own path.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Absolute <see cref="Directory"/> path, where assets are located, before their own path.
    /// </summary>
    public string? PathToContent { get; set; }

    /// <summary>
    /// Constant value to track changes.
    /// </summary>
    public const string Version = "1.5";

    /// <summary>
    /// Executes the task, called by MSBuild.
    /// </summary>
    /// <returns><see langword="true"/> on success, <see langword="false"/> otherwise.</returns>
    public override bool Execute()
    {
        if (Effects is null)
        {
            LogInfo("Effects was not specified! Must be list of file paths, relative to *PathToContent*, of files which should be compiled");
            Environment.Exit(3);
            return false;
        }
        if (OutputPath is null)
        {
            LogInfo("OutputPath was not specified! Must be absolute directory path, that is root path to assets. Assets' own relative path is appended to this to get a final output directory.");
            Environment.Exit(3);
            return false;
        }
        if (PathToContent is null)
        {
            LogInfo("PathToContent was not specified! Must be absolute directory path, where assets are located, without their own relative path.");
            Environment.Exit(3);
            return false;
        }

        LogInfo($"Running CompileEffectsTask v{Version}");

        string[] effectFiles = Effects.Split(';');
        int exitCode = EffectCompiler.Compile(effectFiles, OutputPath, PathToContent, Log);
        if (exitCode != 0) Environment.Exit(exitCode);
        return true;
    }

    private void LogInfo(string text)
    {
        if (!string.IsNullOrEmpty(text))
            Log.LogMessage(MessageImportance.High, text);
    }
}
