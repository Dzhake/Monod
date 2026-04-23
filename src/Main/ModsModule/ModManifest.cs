using System.Text.Json.Serialization;

namespace Monod.ModsModule;

/// <summary>
/// Represents serialized configuration for a <see cref="Mod"/>,
/// </summary>
public class ModManifest
{
    /// <summary>
    /// Mod's id — unique name and version.
    /// </summary>
    [JsonInclude]
    public required ModId Id;

    /// <summary>
    /// <see cref="File"/> path, relative to mod's directory, pointing at .dll file related to mod.
    /// </summary>
    [JsonInclude]
    public string? AssemblyFile;

    /// <summary>
    /// Dependencies of the mod, which must be loaded for mod to run.
    /// </summary>
    [JsonInclude]
    public List<ModDep>? Deps;
}
