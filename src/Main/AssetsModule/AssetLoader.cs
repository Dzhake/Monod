using Microsoft.Extensions.FileSystemGlobbing;
using Monod.AssetsModule.Commands;
using Monod.Shared;
using Serilog;

namespace Monod.AssetsModule;

/// <summary>
/// Provides methods for loading assets from a directory.
/// </summary>
public class AssetLoader
{
    /// <summary>
    /// Manager that uses this <see cref="AssetLoader"/>.
    /// </summary>
    public AssetManager Manager = null!;

    /// <summary>
    /// The full path to the directory that this asset loader loads assets from.
    /// </summary>
    public readonly string DirectoryPath;

    public AssetLoader(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        DirectoryPath = Path.GetFullPath(directoryPath);
    }

    /// <summary>
    /// Amount of currently loaded assets.
    /// </summary>
    public int LoadedAssets => Cache.Count;

    /// <summary>
    /// List of <see cref="MatcherInfo"/>s loaded from asset manifests, in order how they should be applied (first one to match means the one to use).
    /// </summary>
    public MatcherInfo[]? Matchers;

    /// <summary>
    /// Cache of the assets, with key being path of the asset in this <see cref="AssetManager"/> and value is the asset.
    /// </summary>
    protected readonly Dictionary<string, object> Cache = new();

    /// <summary>
    /// Lock for <see cref="Cache"/>.
    /// </summary>
    protected ReaderWriterLockSlim CacheLock = new(LockRecursionPolicy.SupportsRecursion);

    protected Queue<AssetLoaderCommand> Commands = new();
    public AssetLoaderCommand? ActiveCommand;
    public bool LoadingInactive => ActiveCommand?.IsFinished ?? true;
    public int CommandsLeft => Commands.Count;


    public ReaderWriterLockSlim CommandsLock = new(LockRecursionPolicy.SupportsRecursion);

    /// <inheritdoc />
    public override string ToString() => Manager.ToString();

    protected void TryAddCommand(AssetLoaderCommand command)
    {
        Log.Information("{Text}", command.GetText());
        Assets.IncrementTotalCommandsCount();
        try
        {
            CommandsLock.EnterWriteLock();

            if (LoadingInactive)
                RunCommand(command);
            else
                Commands.Enqueue(command);
        }
        finally
        {
            CommandsLock.ExitWriteLock();
        }
    }

    public void RunNextCommand()
    {
        try
        {
            CommandsLock.EnterWriteLock();
            if (Commands.Count == 0) return;
            AssetLoaderCommand command = Commands.Dequeue();
            RunCommand(command);
        }
        finally
        {
            CommandsLock.ExitWriteLock();
        }
    }

    protected void RunCommand(AssetLoaderCommand command)
    {
        ActiveCommand = command;
        MainThread.Add(Task.Run(command.Run));
    }

    public void EnqueueLoadAssetManifests()
    {
        TryAddCommand(new LoadAssetManifestsCommand(this));
    }

    public void EnqueueLoadAssets() => EnqueueLoadAssetsInDir("");
    public void EnqueueLoadAssetsInDir(string path) => TryAddCommand(new LoadAssetsInDirCommand(path, this));

    /// <summary>
    /// Matches the given <paramref name="path"/> against <see cref="Matchers"/>.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/>.</param>
    /// <returns>List of properties the asset at the given <paramref name="path"/> should have.</returns>
    protected List<Dictionary<int, object>> MatchPath(string path)
    {
        List<Dictionary<int, object>> result = new();
        if (Matchers is null) return result;
        foreach (MatcherInfo matcherInfo in Matchers)
            if (matcherInfo.PathMatcher.Match(path).HasMatches) result.Add(matcherInfo.Properties);
        return result;
    }

    /// <summary>
    /// Filter asset paths for <see cref="LoadAssetsInDirCommand"/> using <see cref="AssetManager.Filter"/> and filtering entries that are already in the <see cref="Cache"/>.
    /// </summary>
    /// <param name="assetPaths"><see cref="IEnumerable{string}"/> containing asset paths in this asset manager.</param>
    /// <returns><paramref name="assetPaths"/> filtered using <see cref="AssetManager.Filter"/> and without entries that are already in the <see cref="Cache"/>.</returns>
    public IEnumerable<string> FilterPaths(IEnumerable<string> assetPaths)
    {
        Func<string, bool> filter;
        if (Manager.Filter is null)
            filter = path => !Cache.ContainsKey(path);
        else
            filter = path => !Cache.ContainsKey(path) && Manager.Filter.ShouldLoad(path);

        try
        {
            CacheLock.EnterReadLock();
            return assetPaths.Where(filter);
        }
        finally
        {
            CacheLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Load asset at the specified path in the cache synchronously, <b>replacing</b> already loaded assets. Useful for quickly loading fonts for the startup loading screen.
    /// </summary>
    public void LoadAsset(string path)
    {
        using AssetStream? assetStream = LoadAssetStream(path);

        if (assetStream is null) //When asset was deleted/renamed?
        {
            Log.Debug("{This}: Unloaded asset at {Path}.", this, path);
            RemoveFromCache(path);
            return;
        }

        if (assetStream.Value.Type == AssetType.Ignore)
        {
            Log.Debug("{This}: Ignoring asset at {Path}.", this, path);
            return;
        }


        AssetInfo assetInfo = assetStream.Value.ToInfo(MatchPath(path).ToArray(), path);
        AssetParser? parser = GetParser(assetInfo);
        if (parser is null)
        {
            Log.Warning("Could not find parser for the asset with type {Type} at path {Path}. Verify that parser is specified correctly and that asset has a supported format.", assetInfo.Type, path);
            return;
        }

        object? asset = parser(assetInfo, Manager);
        if (asset is null) //failed to load (logged by parser)/loaded to somewhere else by parser
        {
            return;
        }

        LoadIntoCache(path, asset);
    }

    /// <summary>
    /// Get <see cref="AssetParser"/> based on the specified <paramref name="assetInfo"/>.
    /// </summary>
    /// <param name="assetInfo">Info about asset to get parser for.</param>
    /// <returns><see cref="AssetParser"/> based on the specified <paramref name="assetInfo"/> or null if parser could not be determined.</returns>
    private static AssetParser? GetParser(AssetInfo assetInfo)
    {
        AssetParser? parser = assetInfo.GetProperty<AssetParser>(AssetProps.Parser);
        if (parser is null) Assets.DefaultParsers.TryGetValue(assetInfo.Type, out parser);
        return parser;
    }

    /// <summary>
    /// Load the specified <paramref name="asset"/> into the cache with the <paramref name="path"/> as key.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <param name="asset">Asset to load into the cache.</param>
    protected void LoadIntoCache(string path, object asset)
    {
        try
        {
            CacheLock.EnterWriteLock();
            if (Cache.TryGetValue(path, out object? oldAsset) && oldAsset is IDisposable disposable) disposable.Dispose();
            Cache[path] = asset;
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Remove the specified key (path) from the <see cref="Cache"/>.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    public void RemoveFromCache(string path)
    {
        try
        {
            CacheLock.EnterWriteLock();
            if (Cache.TryGetValue(path, out object? asset) && asset is IDisposable disposable) disposable.Dispose();
            Cache.Remove(path);
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }

        Log.Debug("{This}: Removed asset from the cache: {Path}", this, path);
    }

    /// <summary>
    /// Remove all assets in the <paramref name="dirPath"/> from cache (unload them).
    /// </summary>
    /// <param name="dirPath">Directory path in this <see cref="AssetLoader"/>.</param>
    public void RemoveDirFromCache(string dirPath)
    {
        if (!dirPath.EndsWith(Path.DirectorySeparatorChar) && !dirPath.EndsWith(Path.AltDirectorySeparatorChar)) dirPath += Path.DirectorySeparatorChar;

        try
        {
            int unloadedAssetsCount = 0;
            CacheLock.EnterWriteLock();
            foreach (string asset in Cache.Keys)
            {
                if (asset.StartsWith(dirPath))
                {
                    Cache.Remove(asset);
                    unloadedAssetsCount++;
                }
            }

            Log.Information("{This}: Unloaded {Count} assets", this, unloadedAssetsCount);
            Cache.TrimExcess();
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Remove all <paramref name="paths"/> from <see cref="Cache"/>.
    /// </summary>
    /// <param name="paths">Array of asset paths in this <see cref="AssetLoader"/>.</param>
    public void RemoveFromCache(string[] paths)
    {
        try
        {
            CacheLock.EnterWriteLock();
            foreach (string path in paths)
                Cache.Remove(path);
            Cache.TrimExcess();
            GC.Collect();
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
        Log.Information("{This}: Unloaded {Count} assets", this, paths.Length);
    }

    /// <summary>
    /// Load stream of the asset at the specified path. Loads new asset each time.
    /// </summary>
    /// <param name="path">Path of the asset to load in this <see cref="AssetLoader"/>.</param>
    /// <returns>New <see cref="AssetStream"/> with the <see cref="AssetStream.Stream"/> reading the asset and the type respective to the loaded asset, or null if the asset was not found.</returns>
    protected AssetStream? LoadAssetStream(string path)
    {
        path = path.Replace('\\', '/');
        string fullPath = Path.Join(DirectoryPath, path);
        if (!File.Exists(fullPath))
            return null;
        try
        {
            return new(File.OpenRead(fullPath), AssetsUtils.DetectTypeByPath(path));
        }
        catch (Exception exception)
        {
            Log.Error(exception, "An exception occured while trying to open a file:");
            return null;
        }
    }

    /// <summary>
    /// Load stream of the asset at the specified path asynchronously. Loads new asset each time.
    /// </summary>
    /// <param name="path">Path of the asset to load in this <see cref="AssetLoader"/>.</param>
    /// <returns>New <see cref="AssetStream"/> with the <see cref="AssetStream.Stream"/> reading the asset and the type respective to the loaded asset, or null if the asset was not found.</returns>
    protected Task<AssetStream?> LoadAssetStreamAsync(string path) => new(() => LoadAssetStream(path));

    /// <summary>
    /// Get asset at the specified path from the cache.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <returns>The asset, or null if asset at the specified <paramref cref="path"/> was not found.</returns>
    public object? GetAsset(string path)
    {
        try
        {
            CacheLock.EnterReadLock();
            return Cache.GetValueOrDefault(path);
        }
        finally
        {
            CacheLock.ExitReadLock();
        }
    }
}