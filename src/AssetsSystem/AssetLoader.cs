using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Monod.Utils.General;

namespace Monod.AssetsSystem;

/// <summary>
/// Provides methods for loading assets from an external source.
/// </summary>
public abstract class AssetLoader
{
    /// <summary>
    /// Manager that uses this <see cref="AssetLoader"/>.
    /// </summary>
    public AssetManager Manager;

    /// <summary>
    /// Total amount of asset manifests that are loaded or are loading right now.
    /// </summary>
    public int TotalAssetManifests;

    /// <summary>
    /// Amount of currently loaded asset manifests.
    /// </summary>
    public int LoadedAssetManifests;

    /// <summary>
    /// Total amount of assets that are loaded or are loading right now.
    /// </summary>
    public int TotalAssets;

    /// <summary>
    /// Amount of currently loaded assets.
    /// </summary>
    public int LoadedAssets;

    /// <summary>
    /// List of <see cref="MatcherInfo"/>s loaded from asset manifests, in order how they should be applied (first one to match means the one to use).
    /// </summary>
    public MatcherInfo[]? Matchers;

    /// <summary>
    /// String that will be shown as name of the asset manager.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public override string ToString() => DisplayName;

    /// <summary>
    /// Load all asset manifests for this <see cref="AssetLoader"/> asynchronously, can be used for reloading.
    /// </summary>
    public abstract void LoadAssetManifests();

    /// <summary>
    /// Parse matchers from the specified asset manifest (as a <paramref name="stream"/>) with the specified <paramref name="relativePath"/> of the manifest.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> that reads the asset manifest.</param>
    /// <param name="relativePath">Path of the asset manifest relative to <see cref="AssetManager"/>'s root directory. Used to prefix each match with it, making matchers use subdirectory of the manifest.</param>
    /// <returns>List of <see cref="MatcherInfo"/>s parsed from the specified asset manifest.</returns>
    protected static List<MatcherInfo> ParseAssetManifest(Stream stream, string relativePath)
    {
        var document = JsonDocument.Parse(stream, Json.DocumentCommon);

        List<MatcherInfo> result = new();

        foreach (var match in document.RootElement.EnumerateObject())
        {
            var properties = new Dictionary<int, object>();

            foreach (var inner in match.Value.EnumerateObject())
            {
                int id = Assets.PropNameToId(inner.Name);
                properties[id] = Assets.ParseAssetProp(inner.Value.GetRawText(), id);
            }

            result.Add(new MatcherInfo(Globbing.MatcherFromString(match.Name, relativePath), properties));
        }

        return result;
    }
    
    /// <summary>
    /// Load all assets in the cache asynchronously, <b>without replacing</b> already loaded assets.
    /// </summary>
    public abstract void LoadAssets();

    /// <summary>
    /// Load asset at the specified path in the cache synchronously, <b>without replacing</b> already loaded assets. Useful for quickly loading fonts for the startup loading screen.
    /// </summary>
    public abstract void LoadAsset(string path);

    /// <summary>
    /// Load all assets in the cache asynchronously, replacing already loaded assets.
    /// </summary>
    public abstract void ReloadAssets();
    
    /// <summary>
    /// Get asset at the specified path from the cache. <see cref="LoadAssets"/> must be called first.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <returns>The asset, or <see langword="null"/> if asset at the specified <paramref cref="path"/> was not found.</returns>
    public abstract object? GetAsset(string path);
}