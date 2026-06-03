#if !TASK
#else
using System;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
#endif
using System.ComponentModel;
using System.Diagnostics;

#if TASK
namespace Monod.MSBuild;
#else
namespace Monod.AssetsModule;
#endif

/// <summary>
/// A wrapper around "mgfxc" dotnet tool, that is used to compile provided effects during and build and at runtime.
/// </summary>
public static class EffectCompiler
{
#if TASK
    public static TaskLoggingHelper Log;
#endif

    /// <summary>
    /// Compile the provided <paramref name="effectFiles"/> using mgfxc, and return the 'exit code' (see 'returns' part of the docs for more info).
    /// </summary>
    /// <param name="effectFiles">Array of paths to .fx files. Might be absolute or relative to <paramref name="rootDir"/>.</param>
    /// <param name="outputDir">Directory, to where output compiled .mgfx files. Output files will have same relative path to <paramref name="outputDir"/> as input files to <paramref name="rootDir"/>.</param>
    /// <param name="rootDir">Root directory of <paramref name="effectFiles"/>, to which the files are relative.</param>
    /// <returns>Exit codes: 0 - success, 1 - exception was caught and logged</returns>
    public static int Compile(string[] effectFiles, string outputDir, string rootDir)
    {
        if (effectFiles.Length == 0)
        {
            LogInfo("No effect files provided.");
            return 0;
        }

        for (int effectIdx = 0; effectIdx < effectFiles.Length; effectIdx++)
            effectFiles[effectIdx] = effectFiles[effectIdx].Replace('\\', '/');

        outputDir = outputDir.Replace('\\', '/');
        rootDir = rootDir.Replace('\\', '/');

#if NET5_0_OR_GREATER
        Assets.Logger.Information("Compiling effects: {Effects}", string.Join(';', effectFiles.Select(file => Path.GetRelativePath(rootDir, file))));
#else
        LogInfo($"Compiling effects: {string.Join(";", effectFiles)}");
#endif

        Process[] processes = new Process[effectFiles.Length];
        string[] errorOutputs = new string[effectFiles.Length];

        //Start compile processes
        for (int i = 0; i < processes.Length; i++)
        {
            string effect = effectFiles[i];
            string inputPath = Path.Combine(rootDir, effect);
            string outputPath = Path.ChangeExtension(Path.Combine(outputDir, effect), ".mgfx");

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
                if (exception is Win32Exception) //Probably "The system cannot find the file specified"? I hope that's the only case.
                {
                    LogInfo("(Probably) MGFXC could not be started! You need to install it as global dotnet tool, and check that you can run it from CMD via 'mgfxc' (global dotnet tools dir should be at PATH).");
                }
#if TASK
                LogError(exception.ToString());
#else
                Assets.Logger.Error(exception, "Failed to compile effect:");
#endif
                return 1;
            }
        }

        for (int i = 0; i < effectFiles.Length; i++)
        {
            try
            {
                processes[i]?.WaitForExit();
                if (errorOutputs[i] is not null && errorOutputs[i].Length != 0 && processes[i].ExitCode != 0)
                {
#if TASK
                    ParseOutput(errorOutputs[i]);
                    LogError($"{effectFiles[i]}\n{errorOutputs[i]}mgfx compiler exited with code {processes[i].ExitCode}");
#else
                    Assets.Logger.Error("{Effect}\n{ErrorOutput}mgfx compiler exited with code {ExitCode}", effectFiles[i], errorOutputs[i], processes[i].ExitCode);
#endif
                }
                else
                {
#if TASK
                    LogInfo($"{effectFiles[i]}: sucessfully compiled");
#else
                    Assets.Logger.Information("{EffectFile}: sucessfully compiled", effectFiles[i]);
#endif
                }
            }
            catch (Exception exception)
            {
                if (exception is Win32Exception) //Probably "The system cannot find the file specified"? I hope that's the only case.
                {
                    LogInfo("(Probably) MGFXC could not be started! You need to install it as global dotnet tool, and check that you can run it from CMD via 'mgfxc' (global dotnet tools dir should be at PATH).");
                }
#if TASK
                LogError(exception.ToString());
#else
                Assets.Logger.Error(exception, "Failed to compile effect:");
#endif
                return 1;
            }
        }

        LogInfo("Finished compiling effects");
        return 0;
    }

#if TASK
    private static void LogInfo(string text)
    {
        if (!string.IsNullOrEmpty(text))
            Log.LogMessage(MessageImportance.High, text);
    }

    private static void LogInfo(object? sender, DataReceivedEventArgs e)
    {
        LogInfo(e.Data);
    }

    private static void LogError(string text)
    {
        if (!string.IsNullOrEmpty(text))
            Log.LogError(text);
    }

    private static void LogError(object sender, DataReceivedEventArgs e)
    {
        LogError(e.Data);
    }

    /*Sample Output:
         *  Dependency: N:/repos/DerelictDimension/src/Content/effects/Card.fxh
         *  N:/repos/DerelictDimension/src/Content/effects/CardModify.fx(58,9,58,36): warning X3206: implicit truncation of vector type
         *  N:/repos/DerelictDimension/src/Content/effects/CardModify.fx(58,9,58,36): error X3017: cannot implicitly convert from 'const float2' to 'float3'
         *
         *  Failed to compile 'N:/repos/DerelictDimension/src/Content/effects/CardModify.fx'!
         */
    private static void ParseOutput(string s)
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

            string fileAndCoords = string.Join(":", parts, 0, kindIndex);
            string message = string.Join(":", parts, kindIndex + 1, parts.Length - kindIndex - 1).Trim();
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
#else
    private static void LogInfo(object? sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data)) Assets.Logger.Information("{Info}", e.Data);
    }

#pragma warning disable Serilog004 // Constant MessageTemplate verifier this is just a wrapper, it shouldn't be a constant.
    private static void LogInfo(string text) => Assets.Logger.Information(text);
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
#endif
}