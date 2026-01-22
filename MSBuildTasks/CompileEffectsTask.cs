using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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
    public const string Version = "1.3";

    /// <summary>
    /// Executes the task, called by MSBuild.
    /// </summary>
    /// <returns><see langword="true"/> on success, <see langword="false"/> otherwise.</returns>
    public override bool Execute()
    {
        if (Effects is null)
        {
            Log.LogMessage(MessageImportance.High, "Effects was not specified! Must be list of file paths, relative to *PathToContent*, of files which should be compiled");
            Environment.Exit(3);
            return false;
        }
        if (OutputPath is null)
        {
            Log.LogMessage(MessageImportance.High, "OutputPath was not specified! Must be absolute directory path, that is root path to assets. Assets' own relative path is appended to this to get a final output directory.");
            Environment.Exit(3);
            return false;
        }
        if (PathToContent is null)
        {
            Log.LogMessage(MessageImportance.High, "PathToContent was not specified! Must be absolute directory path, where assets are located, without their own relative path.");
            Environment.Exit(3);
            return false;
        }

        Log.LogMessage(MessageImportance.High, $"Running CompileEffectsTask v{Version}");

        Effects = Effects.Replace('\\', '/');
        OutputPath = OutputPath.Replace('\\', '/');
        PathToContent = PathToContent.Replace('\\', '/');

        Log.LogMessage(MessageImportance.High, $"Effects: {Effects}");

        string[] effectFiles = Effects.Split(';');
        Process[] processes = new Process[effectFiles.Length];

        //Start compile processes
        for (int i = 0; i < processes.Length; i++)
        {
            string effect = effectFiles[i];
            string inputPath = Path.Combine(PathToContent, effect);
            string outputPath = Path.ChangeExtension(Path.Combine(OutputPath, effect), ".mgfx");
            if (File.GetLastWriteTimeUtc(inputPath) < File.GetLastWriteTimeUtc(outputPath)) continue;

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "");
            try
            { 
                ProcessStartInfo startInfo = new()
                {
                    FileName = "mgfxc",
                    Arguments = $"\"{inputPath}\" \"{outputPath}\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                processes[i] = new()
                {
                    StartInfo = startInfo
                };
                processes[i].Start();
            }
            catch (Exception exception)
            {
                int exit = 1;
                if (exception is Win32Exception) //Probably "The system cannot find the file specified"? I hope that's the only case.
                {
                    Log.LogMessage(MessageImportance.High, "(Probably) MGFXC could not be started! You need to install it as global dotnet tool, and check that you can run it from CMD via 'mgfxc' (global dotnet tools dir should be at PATH).");
                    exit = 2;
                }
                Log.LogMessage(MessageImportance.High, exception.ToString());
                Environment.Exit(exit);
                return false;
            }
        }

        for (int i = 0; i < effectFiles.Length; i++)
        {
            try
            {
                processes[i]?.WaitForExit();
            }
            catch (Exception exception)
            {
                int exit = 1;
                if (exception is Win32Exception) //Probably "The system cannot find the file specified"? I hope that's the only case.
                {
                    Log.LogMessage(MessageImportance.High, "(Probably) MGFXC could not be started! You need to install it as global dotnet tool, and check that you can run it from CMD via 'mgfxc' (global dotnet tools dir should be at PATH).");
                    exit = 2;
                }
                Log.LogMessage(MessageImportance.High, exception.ToString());
                Environment.Exit(exit);
                return false;
            }
        }

        Log.LogMessage(MessageImportance.High, "Finished compiling effects");
        return true;
    }
}
