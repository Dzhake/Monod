using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Create symlink at <see cref="Destination"/> poiting to <see cref="Source"/>.
/// </summary>
public sealed class CreateSymlink : Task
{
    /// <summary>
    /// Source of the symlink; where the symlink points.
    /// </summary>
    [Required]
    public string? Source { get; set; }

    /// <summary>
    /// Destionation of the symlink; where the symlink is created.
    /// </summary>
    [Required]
    public string? Destination { get; set; }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Determine whether file/directory at the specified <paramref name="path"/> is a symlink.
    /// </summary>
    /// <param name="path">Path of the file/directory.</param>
    /// <returns>Whether file/directory at the specified <paramref name="path"/> is a symlink.</returns>
    private bool IsSymlink(string path)
    {
        FileInfo file = new FileInfo(path);
        return file.LinkTarget != null;
    }
#endif


    /// <inheritdoc/>
    public override bool Execute()
    {
        try
        {

            if (Source is null)
            {
                Log.LogMessage(MessageImportance.High, "Source was not specified! Must be single absolute file path, where the symlink will point.");
                Environment.Exit(3);
                return false;
            }
            if (Destination is null)
            {
                Log.LogMessage(MessageImportance.High, "Destination was not specified! Must be single absolute file path, where the symlink will be created.");
                Environment.Exit(3);
                return false;
            }

            if (File.Exists(Destination))
            {
#if NET6_0_OR_GREATER
                if (IsSymlink(Destination)) return true;
#endif
                File.Delete(Destination);
            }

            if (Directory.Exists(Destination))
            {
#if NET6_0_OR_GREATER
                if (IsSymlink(Destination)) return true;
#endif
                Directory.Delete(Destination, true);
            }

#if NET6_0_OR_GREATER
            if (TryCreateViaDotNet())
                return true;
#endif

            if (TryCreateViaShell())
                return true;

            Log.LogError($"Failed to create symlink to '{Source}' at '{Destination}'");
            return false;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex, true);
            return false;
        }
    }


#if NET6_0_OR_GREATER
    bool TryCreateViaDotNet()
    {
        try
        {
            if (Directory.Exists(Source))
                Directory.CreateSymbolicLink(Destination, Source);
            else
                File.CreateSymbolicLink(Destination, Source);

            Log.LogMessage($"Symlink created: {Source} -> {Destination}"); //default method no need to say "via .NET"
            return true;
        }
        catch (Exception ex)
        {
            Log.LogMessage(MessageImportance.High,
                $"Failed to create symlink via .NET to '{Source}' at '{Destination}': {ex.Message}");
            return false;
        }
    }
#endif

    bool TryCreateViaShell()
    {
        try
        {
            bool isDir = Directory.Exists(Source);

            ProcessStartInfo psi;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string args = isDir
                    ? $"/c mklink /D \"{Destination}\" \"{Source}\""
                    : $"/c mklink \"{Destination}\" \"{Source}\"";

                psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = args
                };
            }
            else
            {
                // Linux / macOS
                string args = $"-s \"{Source}\" \"{Destination}\"";

                psi = new ProcessStartInfo
                {
                    FileName = "ln",
                    Arguments = args
                };
            }

            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            using var p = Process.Start(psi);
            p.WaitForExit();

            if (p.ExitCode == 0)
            {
                Log.LogMessage(MessageImportance.High, $"Symlink created via shell: '{Source}' -> '{Destination}'");
                return true;
            }

            string err = p.StandardError.ReadToEnd();
            Log.LogMessage(MessageImportance.High,
                $"Failed to create symlink via shell for {Source}: {err}");

            return false;
        }
        catch (Exception ex)
        {
            Log.LogMessage(MessageImportance.High,
                $"Shell symlink failed: {ex.Message}");
            return false;
        }
    }
}
