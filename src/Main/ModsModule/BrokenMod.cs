namespace Monod.ModsModule;

/// <summary>
/// Helper class for quickly creating mods with common failure problems.
/// </summary>
public sealed class BrokenMod
{
    public string ManifestPath;
    public ModManifest? Manifest;
    public Exception FailureReason;

    public static BrokenMod New(Exception failureReason, string manifestPath, ModManifest manifest) =>
        new()
        {
            ManifestPath = manifestPath,
            Manifest = manifest,
            FailureReason = failureReason,
        };


    public static BrokenMod New(Exception failureReason, string manifestPath) =>
        new()
        {
            ManifestPath = manifestPath,
            FailureReason = failureReason,
        };


    public static BrokenMod FailedToDeserializeManifest(string manifestPath, Exception exception) => New(new($"Failed to deserialize manifest: {exception.Message}"), manifestPath);
    public static BrokenMod FailedToDeserializeManifest(string manifestPath) => New(new("Failed to deserialize manifest"), manifestPath);

    //public static BrokenMod HardDepsNotMet(ModManifest manifest) => New(new("Could not satisfy hard dependencies."), manifest);

    public static BrokenMod ManifestNotFound(string configPath, string modName) => New(new FileNotFoundException("Could not find manifest file", configPath), modName);
}