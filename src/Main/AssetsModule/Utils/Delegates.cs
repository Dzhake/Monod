namespace Monod.AssetsModule.Utils;

/// <summary>
/// A Func(AssetInfo, object?) analog. Returns null if asset could not be parsed.
/// </summary>
public delegate Task<object?> AssetParser(AssetInfo assetInfo, AssetManager manager);