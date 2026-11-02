namespace Monod.AssetsModule;

/// <summary>
/// Context for any operations with the <see cref="AssetLoader.Cache"/>, passed to <see cref="IAsset"/> callbacks.
/// </summary>
/// <param name="Path">Path of the asset in the <see cref="assetLoader"/> (key in the cache).</param>
/// <param name="assetLoader">Asset loader which owns the cache.</param>
public record struct AssetCacheContext(string Path, AssetLoader assetLoader);
