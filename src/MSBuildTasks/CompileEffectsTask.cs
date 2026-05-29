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

        Effects = Effects.Replace('\\', '/');
        OutputPath = OutputPath.Replace('\\', '/');
        PathToContent = PathToContent.Replace('\\', '/');

        LogInfo($"Effects: {Effects}");

        string[] effectFiles = Effects.Split(';');
        Process[] processes = new Process[effectFiles.Length];
        string[] errorOutputs = new string[effectFiles.Length];

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
                //Debugger.Launch();
                ProcessStartInfo startInfo = new()
                {
                    FileName = "mgfxc",
                    Arguments = $"\"{inputPath}\" \"{outputPath}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };

                Process process = new()
                {
                    StartInfo = startInfo,
                };

                processes[i] = process;

                process.OutputDataReceived += LogInfo;
                int iref = i;
                process.ErrorDataReceived += (s, args) => { errorOutputs[iref] ??= ""; errorOutputs[iref] += $"{args.Data}\n"; };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
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
                if (errorOutputs[i] is not null && errorOutputs[i].Length != 0)
                {
                    ParseOutput(errorOutputs[i]);
                    //LogError($"MGFXC1:{effectFiles[i]}\n{errorOutputs[i]}mgfx compiler exited with code {processes[i].ExitCode}");
                }
                else
                {
                    LogInfo($"{effectFiles[i]}: sucessfully compiled");
                }
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
    /*Sample Output:
         *  Dependency: N:/repos/DerelictDimension/src/Content/Effects/Card.fxh
         *  N:/repos/DerelictDimension/src/Content/Effects/CardModify.fx(58,9,58,36): warning X3206: implicit truncation of vector type
         *  N:/repos/DerelictDimension/src/Content/Effects/CardModify.fx(58,9,58,36): error X3017: cannot implicitly convert from 'const float2' to 'float3'
         *
         *  Failed to compile 'N:/repos/DerelictDimension/src/Content/Effects/CardModify.fx'!
         */
    private void ParseOutput(string s)
    {
        if (string.IsNullOrEmpty(s))
            return;

        foreach (string rawLine in s.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            string line = rawLine.TrimEnd('\r');

            string[] parts = line.Split(':');

            if (parts.Length < 3)
                continue;

            int kindIndex = -1;
            bool isWarning = false;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                if (part.Contains(" warning "))
                {
                    kindIndex = i;
                    isWarning = true;
                    break;
                }

                if (part.Contains(" error "))
                {
                    kindIndex = i;
                    break;
                }
            }

            if (kindIndex < 0)
                continue;

            // всё до warning/error — file + coords
            string fileAndCoords = string.Join(":", parts, 0, kindIndex);

            // всё после — message
            string message = string.Join(":", parts, kindIndex + 1, parts.Length - kindIndex - 1).Trim();

            // part = " warning X3206"
            string kindPart = parts[kindIndex].Trim();

            string code;

            if (isWarning)
            {
                code = kindPart.Replace("warning ", "");
            }
            else
            {
                code = kindPart.Replace("error ", "");
            }

            int parenPos = fileAndCoords.LastIndexOf('(');
            int closeParenPos = fileAndCoords.LastIndexOf(')');

            if (parenPos < 0 || closeParenPos < 0 || closeParenPos <= parenPos)
                continue;

            string file = fileAndCoords.Substring(0, parenPos);

            string[] coords = fileAndCoords
                .Substring(parenPos + 1, closeParenPos - parenPos - 1)
                .Split(',');

            if (coords.Length != 4)
                continue;

            if (!int.TryParse(coords[0], out int lineNumber) ||
                !int.TryParse(coords[1], out int columnNumber) ||
                !int.TryParse(coords[2], out int endLineNumber) ||
                !int.TryParse(coords[3], out int endColumnNumber))
            {
                continue;
            }

            if (isWarning)
            {
                Log.LogWarning(
                    subcategory: null,
                    warningCode: code,
                    helpKeyword: null,
                    file: file,
                    lineNumber: lineNumber,
                    columnNumber: columnNumber,
                    endLineNumber: endLineNumber,
                    endColumnNumber: endColumnNumber,
                    message: message);
            }
            else
            {
                Log.LogError(
                    subcategory: null,
                    errorCode: code,
                    helpKeyword: null,
                    file: file,
                    lineNumber: lineNumber,
                    columnNumber: columnNumber,
                    endLineNumber: endLineNumber,
                    endColumnNumber: endColumnNumber,
                    message: message);
            }
        }
    }

    private void LogInfo(string text)
    {
        if (!string.IsNullOrEmpty(text))
            Log.LogMessage(MessageImportance.High, text);
    }

    private void LogInfo(object? sender, DataReceivedEventArgs e)
    {
        LogInfo(e.Data);
    }

    private void LogError(string text)
    {
        if (!string.IsNullOrEmpty(text))
            Log.LogError(text);
    }

    private void LogError(object sender, DataReceivedEventArgs e)
    {
        LogError(e.Data);
    }
}
