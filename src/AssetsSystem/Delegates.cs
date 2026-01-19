namespace Monod.AssetsSystem;

/// <summary>
/// A Func(AssetInfo, object?) analog. Shortcut for shorter code. Returns null if asset could not be parsed.
/// </summary>
public delegate object? AssetParser(AssetInfo assetInfo, AssetManager manager);