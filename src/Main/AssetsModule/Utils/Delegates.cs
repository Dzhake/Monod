namespace Monod.AssetsModule.Utils;

/// <summary>
/// Delegate that converts <see cref="AssetInfo"/> into a valid asset, or returns null if asset could not be parsed.
/// </summary>
public delegate Task<object?> AssetParser(AssetInfo assetInfo, AssetManager manager);