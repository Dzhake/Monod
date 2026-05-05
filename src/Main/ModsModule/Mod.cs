using HarmonyLib;
using Monod.AssetsModule;
using Monod.ModsModule.ModdingOld;
using Serilog;
using System.Reflection;

namespace Monod.ModsModule;

/// <summary>
/// Represents a <see cref="Mod"/> with an assembly, assets, and <see cref="ModListener"/>.
/// </summary>
public sealed class Mod
{
    /// <summary>
    /// CurrentValue status of the mod.
    /// </summary>
    public ModStatus Status = ModStatus.Enabled;

    /// <summary>
    /// Reason the mod failed to load. Not null if <see cref="Status"/> is <see cref="ModStatus.FailedToLoad"/>.
    /// </summary>
    public Exception? FailureReason;

    /// <summary>
    /// <see cref="ModManifest"/> read from config.json of this <see cref="Mod"/>.
    /// </summary>
    public ModManifest Manifest = null!;

    /// <summary>
    /// <see cref="Directory"/> path where this mod is located.
    /// </summary>
    public string Directory = null!;

    /// <summary>
    /// Asset manager for assets of this <see cref="Mod"/>.
    /// </summary>
    public AssetManager? Assets;

    /// <summary>
    /// Used to manage and unload <see cref="Assembly"/> loaded by this <see cref="Mod"/>.
    /// </summary>
    public ModAssemblyLoadContext? AssemblyContext;

    /// <summary>
    /// Used to log mod-specific info by using config's <see cref="ModId.Name"/> as unique id in the log.
    /// </summary>
    public ILogger? LoggerInstance;

    /// <summary>
    /// Used to patch methods, patches made using <see cref="HarmonyInstance"/> will be correctly reloaded when reloading mod assembly. Field is null before <see cref="AssignManifest"/> is called.
    /// </summary>
    public Harmony? HarmonyInstance;

    /// <summary>
    /// Listener for the mod, which contains virtual methods called by <see cref="ModManager"/>.
    /// </summary>
    public ModListener? ExternalMod;

    /// <summary>
    /// Ties <paramref name="config"/> with <see cref="Mod"/>, and sets <see cref="LoggerInstance"/> and <see cref="HarmonyInstance"/> based on <paramref name="config"/>.
    /// </summary>
    /// <param name="config">Manifest to assign to this <see cref="Mod"/>.</param>
    public void AssignManifest(ModManifest config)
    {
        Manifest = config;
        string name = Manifest.Id.Name;
        LoggerInstance = Log.ForContext("Mod", name);
        HarmonyInstance = new(name);
    }

    /// <summary>
    /// Get the name of this mod.
    /// </summary>
    /// <returns>The name of this mod.</returns>
    public string GetName() => Manifest.Id.Name;
}
