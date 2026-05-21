using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
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
    public const string Version = "1.4";

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

        Effects = Effects.Replace('\\', '/');
        OutputPath = OutputPath.Replace('\\', '/');
        PathToContent = PathToContent.Replace('\\', '/');

        LogInfo($"Effects: {Effects}");

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
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };

                processes[i] = new()
                {
                    StartInfo = startInfo,
                    //EnableRaisingEvents = true
                };

                processes[i].OutputDataReceived += LogInfo;

                processes[i].ErrorDataReceived += LogError;

                processes[i].Start();
            }
            catch (Exception exception)
            {
                int exit = 1;
                if (exception is Win32Exception) //Probably "The system cannot find the file specified"? I hope that's the only case.
                {
                    LogInfo("(Probably) MGFXC could not be started! You need to install it as global dotnet tool, and check that you can run it from CMD via 'mgfxc' (global dotnet tools dir should be at PATH).");
                    exit = 2;
                }
                LogInfo(exception.ToString());
                Environment.Exit(exit);
                return false;
            }
        }

        for (int i = 0; i < effectFiles.Length; i++)
        {
            try
            {
                processes[i]?.WaitForExit();
                if ((processes[i]?.ExitCode ?? 0) != 0)
                {
                    Console.WriteLine($"{effectFiles[i]}: compiler exited with code {processes[i].ExitCode}");
                    Environment.Exit(1);
                }
                LogInfo($"{effectFiles[i]}: sucessfully compiled");
            }
            catch (Exception exception)
            {
                int exit = 1;
                if (exception is Win32Exception) //Probably "The system cannot find the file specified"? I hope that's the only case.
                {
                    LogInfo("(Probably) MGFXC could not be started! You need to install it as global dotnet tool, and check that you can run it from CMD via 'mgfxc' (global dotnet tools dir should be at PATH).");
                    exit = 2;
                }
                LogInfo(exception.ToString());
                Environment.Exit(exit);
                return false;
            }
        }

        LogInfo("Finished compiling effects");
        return true;
    }

    private void LogInfo(string text)
    {
        Log.LogMessage(MessageImportance.High, text);
    }

    private void LogInfo(object? sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            Log.LogMessage(MessageImportance.High, e.Data);
    }

    private void LogError(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
            Log.LogError(e.Data);
    }
}
