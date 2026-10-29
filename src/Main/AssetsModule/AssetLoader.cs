using D2Phap.FileWatcherEx;
using Microsoft.Extensions.FileSystemGlobbing;
using Monod.AssetsModule.Commands;
using Monod.AssetsModule.Utils;
using Monod.Utils.Commands;
using Monod.Utils.General;

namespace Monod.AssetsModule;

/// <summary>
/// Provides methods for loading assets from a directory.
/// </summary>
public class AssetLoader : CommandRunner<AssetLoaderCommand>, IDisposable
{
    /// <summary>
    /// Manager that uses this <see cref="AssetLoader"/>.
    /// </summary>
    public AssetManager Manager = null!;

    /// <summary>
    /// The full path to the directory that this asset loader loads assets from.
    /// </summary>
    public readonly string DirectoryPath;

    /// <summary>
    /// <see cref="FileSystemWatcher"/>, used to detect changes in files that are stored in <see cref="DirectoryPath"/>.
    /// </summary>
    private FileSystemWatcherEx? Watcher;

    /// <summary>
    /// Amount of currently loaded assets.
    /// </summary>
    public int LoadedAssets => Cache.Count;

    /// <summary>
    /// List of <see cref="MatcherInfo"/>s loaded from asset manifests, in order how they should be applied (first one to match means the one to use).
    /// </summary>
    public MatcherInfo[]? Matchers = GetDefaultMatchers().ToArray();

    /// <summary>
    /// Cache of the assets, with key being path of the asset in this <see cref="AssetManager"/> and value is the asset.
    /// </summary>
    protected readonly Dictionary<string, object> Cache = new();

    /// <summary>
    /// Lock for <see cref="Cache"/>.
    /// </summary>
    protected ReaderWriterLockSlim CacheLock = new(LockRecursionPolicy.SupportsRecursion);

    /// <inheritdoc />
    public override string ToString() => Manager.ToString();

    /// <summary>
    /// Create a new instance of the <see cref="AssetLoader"/>, loading from the <paramref name="directoryPath"/>.
    /// </summary>
    /// <param name="directoryPath">Directory path, to load assets from.</param>
    public AssetLoader(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);
        DirectoryPath = Path.GetFullPath(directoryPath);
        if (MonodSettings.HotReload) InitializeWatcher();
    }

    /// <summary>
    /// Create and initialize <see cref="Watcher"/> to be active.
    /// </summary>
    private void InitializeWatcher()
    {
        Watcher = new(DirectoryPath);

        Watcher.IncludeSubdirectories = true;

        Watcher.OnChanged += OnFileChanged;
        Watcher.OnCreated += OnFileChanged;
        Watcher.OnDeleted += OnFileDeleted;
        Watcher.OnRenamed += OnFileRenamed;

        //Watcher.SynchronizingObject = (System.ComponentModel.ISynchronizeInvoke?)Watcher;
        Watcher.Start();
    }

    private void OnFileRenamed(object? sender, FileChangedEvent e)
    {
        //TODO (asset manifests, low priority): check if either of files is actually an asset manifest
        string relativePath = Path.GetRelativePath(DirectoryPath, e.FullPath).Replace('\\', '/');
        string relativeOldPath = Path.GetRelativePath(DirectoryPath, e.OldFullPath!).Replace('\\', '/');
        if (Directory.Exists(e.FullPath))
        {
            OnDirectoryModified(relativeOldPath);
            OnDirectoryModified(relativePath);
            EnqueueReloadAssetsInDir(relativePath);
            EnqueueRemoveAssetsInDir(relativeOldPath);
            return;
        }

        OnAssetModified(relativeOldPath);
        OnAssetModified(relativePath);
        EnqueueReloadAsset(relativeOldPath);
        EnqueueReloadAsset(relativePath);
    }

    private void OnFileDeleted(object? sender, FileChangedEvent e)
    {
        //TODO (asset manifests, low priority): reload asset manifests when manifest is deleted
        string relativePath = Path.GetRelativePath(DirectoryPath, e.FullPath).Replace('\\', '/');
        // just reload it both as a dir and as an asset to be safe, there aren't really any reliable ways to determine whether a file or dir was deleted

        OnAssetModified(relativePath);
        OnDirectoryModified(relativePath);
        EnqueueRemoveAssetsInDir(relativePath);
        if (!Directory.Exists(e.FullPath)) EnqueueReloadAsset(relativePath);
    }

    private void OnFileChanged(object? sender, FileChangedEvent e)
    {
        //TODO (asset manifests, low priority): reload asset manifests when manifest is changed
        string relativePath = Path.GetRelativePath(DirectoryPath, e.FullPath).Replace('\\', '/');
        if (Directory.Exists(e.FullPath))
        {
            /*OnDirectoryModified(relativePath);
            EnqueueReloadAssetsInDir(relativePath);*/
            return;
        }

        OnAssetModified(relativePath);
        EnqueueReloadAsset(relativePath);
    }

    private void OnAssetModified(string relativePath)
    {
        AssetFormat format = AssetsUtils.DetectFormatByExtension(Path.GetExtension(relativePath));
        if (format == AssetFormat.Fx)
        {
            EffectBuilder.BuildEffect(relativePath, DirectoryPath);
        }
        else if (format == AssetFormat.Fxh)
        {
            EffectBuilder.BuildEffectsOnFxhChanged(relativePath, DirectoryPath);
        }
    }

    private void OnDirectoryModified(string relativePath)
    {
        EffectBuilder.BuildEffectsOnDirChanged(relativePath, DirectoryPath);
    }

    ///<inheritdoc/>
    public override void AddCommand(AssetLoaderCommand command)
    {
        Assets.IncrementTotalCommandsCount();
        base.AddCommand(command);
    }


    /// <summary>
    /// Add <see cref="LoadAssetManifestsCommand"/> to queue.
    /// </summary>
    public void EnqueueLoadAssetManifests() => AddCommand(new LoadAssetManifestsCommand(this));

    /// <summary>
    /// Add <see cref="LoadAssetsInDirCommand"/> with dir being asset loader's root dir to queue.
    /// </summary>
    public void EnqueueLoadAssets() => EnqueueLoadAssetsInDir("");

    /// <summary>
    /// Add <see cref="LoadAssetsInDirCommand"/> to queue.
    /// </summary>
    /// <param name="path">Directory path in this asset loader, assets from where to load.</param>
    public void EnqueueLoadAssetsInDir(string path) => AddCommand(new LoadAssetsInDirCommand(path, this));

    /// <summary>
    /// Add <see cref="ReloadAssetsInDirCommand"/> to queue.
    /// </summary>
    /// <param name="relativePath">Directory path in this asset loader, assets from where to reload.</param>
    public void EnqueueReloadAssetsInDir(string relativePath) => AddCommand(new ReloadAssetsInDirCommand(relativePath, this));

    /// <summary>
    /// Add <see cref="ReloadAssetCommand"/> to queue.
    /// </summary>
    /// <param name="filePath">File path in this asset loader of the asset to reload.</param>
    public void EnqueueReloadAsset(string filePath) => AddCommand(new ReloadAssetCommand(filePath, this));

    /// <summary>
    /// Add <see cref="RemoveAssetsInDirCommand"/> to queue.
    /// </summary>
    /// <param name="relativePath">Directory path in this asset loader.</param>
    public void EnqueueRemoveAssetsInDir(string relativePath) => AddCommand(new RemoveAssetsInDirCommand(relativePath, this));


    /// <summary>
    /// Load asset at the specified path in the cache synchronously, <b>replacing</b> already loaded assets. Useful for quickly loading fonts for the startup loading screen.
    /// </summary>
    public void LoadAsset(string filePath)
    {
        filePath = filePath.Replace('\\', '/');
        AssetInfo assetInfo = GetAssetInfo(filePath);
        LoadAssetFromAssetInfo(assetInfo).Wait();
    }

    public async Task ReloadAssetAsync(string path)
    {
        await LoadAssetAsync(path);
    }

    /// <summary>
    /// Load asset at the <paramref name="filePath"/> asynchronously, <b>replacing</b> existing assets.
    /// </summary>
    /// <param name="filePath">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <returns>Async task.</returns>
    public async Task LoadAssetAsync(string filePath)
    {
        filePath = filePath.Replace('\\', '/');
        AssetInfo assetInfo = GetAssetInfo(filePath);
        await LoadAssetFromAssetInfo(assetInfo);
    }


    private async Task<bool> LoadAssetFromAssetInfo(AssetInfo assetInfo)
    {
        if (assetInfo.FilePath is null) //When asset was deleted/renamed?
        {
            Assets.Logger.Debug("{This}: Unloaded asset at {Path}.", this, assetInfo.Path);
            RemoveFromCache(assetInfo.Path);
            return false;
        }

        if (assetInfo.Type == AssetType.Ignore)
        {
            Assets.Logger.Debug("{This}: Ignoring asset at {Path}.", this, assetInfo.Path);
            return false;
        }


        assetInfo.PropertiesArray = MatchPath(assetInfo.FilePath).ToArray();
        AssetParser? parser = GetParser(assetInfo);

        if (parser is null)
        {
            Assets.Logger.Warning("Could not find parser for the asset with type {Type} at path {Path}. Verify that parser is specified correctly and that asset has a supported format.", assetInfo.Type, assetInfo.Path);
            return false;
        }

        Task<object?> assetTask = parser(assetInfo, Manager);
        object? asset = await assetTask;
        if (asset is null) //failed to load (logged by parser) / loaded to somewhere else by parser
        {
            return false;
        }

        LoadIntoCache(assetInfo.Path, asset);
        return true;
    }

    protected AssetInfo GetAssetInfo(string filePath)
    {
        string fullPath = Path.Join(DirectoryPath, filePath).Replace('\\', '/');
        if (!File.Exists(fullPath)) return new(null, null, FilePathToLoaderPath(filePath), AssetsUtils.DetectTypeByPath(filePath));
        return new(fullPath, filePath, FilePathToLoaderPath(filePath), AssetsUtils.DetectTypeByPath(filePath));
    }

    /// <summary>
    /// Matches the given <paramref name="path"/> against <see cref="Matchers"/>.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetManager"/>.</param>
    /// <returns>List of properties the asset at the given <paramref name="path"/> should have.</returns>
    protected List<Dictionary<AssetProp, object>> MatchPath(string path)
    {
        List<Dictionary<AssetProp, object>> result = new();
        if (Matchers is null) return result;
        foreach (MatcherInfo matcherInfo in Matchers)
            if (matcherInfo.PathMatcher.Match(path).HasMatches) result.Add(matcherInfo.Properties);
        return result;
    }

    public static List<MatcherInfo> GetDefaultMatchers()
    {
        return [
            new MatcherInfo(Globbing.MatcherFromString("**.prefab.json"), new(){ { AssetProps.Parser, (AssetParser)AssetParsers.Prefab } }),
        ];
    }

    public static string FilePathToLoaderPath(string filePath)
    {
        return Path.ChangeExtension(filePath, null);
    }


    /// <summary>
    /// Filter asset paths for <see cref="LoadAssetsInDirCommand"/> using <see cref="AssetManager.Filter"/> and filtering entries that are already in the <see cref="Cache"/>.
    /// </summary>
    /// <param name="assetPaths"><see cref="IEnumerable{T}">IEnumerable&lt;string&gt;</see> of asset paths in this asset manager.</param>
    /// <returns><paramref name="assetPaths"/> filtered using <see cref="AssetManager.Filter"/> and without entries that are already in the <see cref="Cache"/>.</returns>
    public IEnumerable<string> FilterPathsNonReplacing(IEnumerable<string> assetPaths)
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
    /// Filter asset paths for <see cref="LoadAssetsInDirCommand"/> using <see cref="AssetManager.Filter"/>.
    /// </summary>
    /// <param name="assetPaths"><see cref="IEnumerable{T}">IEnumerable&lt;string&gt;</see> of asset paths in this asset manager.</param>
    /// <returns><paramref name="assetPaths"/> filtered using <see cref="AssetManager.Filter"/>.</returns>
    public IEnumerable<string> FilterPaths(IEnumerable<string> assetPaths)
    {
        if (Manager.Filter is null)
            return assetPaths;

        return assetPaths.Where(Manager.Filter.ShouldLoad);
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

        Assets.Logger.Debug("{This}: Removed asset from the cache: {Path}", this, path);
    }

    /// <summary>
    /// Remove all assets in the <paramref name="dirPath"/> from cache (unload them).
    /// </summary>
    /// <param name="dirPath">Directory path in this <see cref="AssetLoader"/>.</param>
    public void RemoveDirFromCache(string dirPath)
    {
        if (!dirPath.EndsWith('/')) dirPath += '/';

        try
        {
            CacheLock.EnterWriteLock();
            foreach (string asset in Cache.Keys)
            {
                if (asset.StartsWith(dirPath))
                    Cache.Remove(asset);
            }

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
        }
        finally
        {
            CacheLock.ExitWriteLock();
        }
        Assets.Logger.Information("{This}: Unloaded {Count} assets", this, paths.Length);
    }




    /// <summary>
    /// Get asset at the specified path from the cache.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <returns>The asset, or null if asset at the specified <paramref name="path"/> was not found.</returns>
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

    public void Dispose()
    {
        Watcher?.Dispose();
        CacheLock.Dispose();
        Cache?.Clear();
        Manager = null!;
        Matchers = null!;

        Assets.ReloadThisFrame = true;
        Assets.EmitReloadEvent();
    }
}