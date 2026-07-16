using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;

namespace Monod.MSBuild;

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
                Log.LogError("Source was not specified! Must be single absolute file (directory) path, where the symlink will point.");
                return false;
            }

            if (Destination is null)
            {
                Log.LogError("Destination was not specified! Must be single absolute file (directory) path, where the symlink will be created.");
                return false;
            }

            Source = Source.Replace('\\', '/');
            Source = Source.TrimEnd('/');

            Destination = Destination.Replace('\\', '/');
            Destination = Destination.TrimEnd('/');


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

            Log.LogError($"Failed to create symlink at '{Destination}' to '{Source}'");
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

            Log.LogWarning($"Symlink created: {Destination} -> {Source}");
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to create symlink (via dotnet) at {Destination} to {Source}: {ex.Message}");
            return false;
        }
    }
#endif

    bool TryCreateViaShell()
    {
        try
        {
            bool isDir = Directory.Exists(Source);
            NativeSymlink.Create(Destination!, Source!, isDir);
            Log.LogWarning($"Created symlink via shell: {Destination} -> {Source}");
            //TODO: figure out why messages are not logged. Currently i'll just use warnings.
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to create symlink (via shell) at {Destination} to {Source}: {ex.Message}");
            return false;
        }
    }
}
