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
    /// Total amount of "assets.json" files that are loaded or are loading right now.
    /// </summary>
    public int TotalAssetManifests;

    /// <summary>
    /// Amount of currently loaded "assets.json" files.
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

    public MatcherInfo[] Matchers;

    /// <summary>
    /// String that will be shown as name of the asset manager.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public override string ToString() => DisplayName;

    /// <summary>
    /// Load all "assets.json" for this <see cref="AssetLoader"/> asynchronously, can be used for reloading.
    /// </summary>
    public abstract void LoadAssetManifests();

    protected static void LoadAssetManifest(Stream manifest, string relativePath)
    {
        var document = JsonDocument.Parse(manifest, Json.DocumentCommon);

        var matchers = new List<MatcherInfo>();

        foreach (var match in document.RootElement.EnumerateObject())
        {
            var properties = new Dictionary<int, object>();

            foreach (var inner in match.Value.EnumerateObject())
            {
                int id = Assets.PropNameToId(inner.Name);
                properties[id] = Assets.ParseAssetProp(inner.Value.GetRawText(), id);
            }

            matchers.Add(new MatcherInfo(Globbing.MatcherFromString(match.Name, relativePath), properties));
        }
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