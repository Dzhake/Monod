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

    /// <summary>
    /// <see cref="FileSystemWatcher"/>, used to detect changes in files that are stored in <see cref="DirectoryPath"/>.
    /// </summary>
    private FileSystemWatcher? Watcher;

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

    /// <summary>
    /// Queue of commands to run. Commands are run when a new command is enqueued (by <see cref="TryAddCommand"/>) or when a command is finished (by <see cref="AssetLoaderCommand.OnFinished"/>). Access only with <see cref="CommandsLock"/>.
    /// </summary>
    protected Queue<AssetLoaderCommand> Commands = new();

    /// <summary>
    /// Commands that is currently being run on another thread.
    /// </summary>
    public AssetLoaderCommand? ActiveCommand;

    /// <summary>
    /// Whether this <see cref="AssetLoader"/> currently doesn't execute any commands.
    /// </summary>
    public bool LoadingInactive => (ActiveCommand?.IsFinished ?? true) && CommandsLeft == 0;

    /// <summary>
    /// Amount of commands left in the queue.
    /// </summary>
    public int CommandsLeft => Commands.Count;

    /// <summary>
    /// Lock for <see cref="Commands"/>.
    /// </summary>
    public ReaderWriterLockSlim CommandsLock = new(LockRecursionPolicy.SupportsRecursion);

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

        Watcher.Changed += OnFileChanged;
        Watcher.Created += OnFileChanged;
        Watcher.Deleted += OnFileDeleted;
        Watcher.Renamed += OnFileRenamed;

        Watcher.EnableRaisingEvents = true;
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        string relativePath = Path.GetRelativePath(DirectoryPath, e.FullPath).Replace('\\', '/');
        string relativeOldPath = Path.GetRelativePath(DirectoryPath, e.OldFullPath).Replace('\\', '/');
        if (Directory.Exists(e.FullPath))
        {
            EnqueueReloadAssetsInDir(relativePath);
            EnqueueReloadAssetsInDir(relativeOldPath);
            return;
        }

        EnqueueReloadAsset(relativeOldPath);
        EnqueueReloadAsset(relativePath);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        string relativePath = Path.GetRelativePath(DirectoryPath, e.FullPath).Replace('\\', '/');
        // just reload it both as a dir and as an asset to be safe, there aren't really any reliable way to determine whether a file or dir was deleted

        EnqueueRemoveAssetsInDir(relativePath);
        if (!Directory.Exists(e.FullPath)) EnqueueReloadAsset(relativePath);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        string relativePath = Path.GetRelativePath(DirectoryPath, e.FullPath).Replace('\\', '/');
        if (Directory.Exists(e.FullPath))
        {
            EnqueueReloadAssetsInDir(relativePath);
            return;
        }

        EnqueueReloadAsset(relativePath);
    }


    /// <summary>
    /// Add the <paramref name="command"/> to the <see cref="Commands"/>, or run it, if <see cref="LoadingInactive"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="command"></param>
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

    /// <summary>
    /// Run the next command in queue.
    /// </summary>
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

    /// <summary>
    /// Run the <paramref name="command"/> on another thread.
    /// </summary>
    /// <param name="command">Command to run.</param>
    protected void RunCommand(AssetLoaderCommand command)
    {
        ActiveCommand = command;
        MainThread.Add(Task.Run(command.Run));
    }


    /// <summary>
    /// Add <see cref="LoadAssetManifestsCommand"/> to queue.
    /// </summary>
    public void EnqueueLoadAssetManifests() => TryAddCommand(new LoadAssetManifestsCommand(this));

    /// <summary>
    /// Add <see cref="LoadAssetsInDirCommand"/> with dir being asset loader's root dir to queue.
    /// </summary>
    public void EnqueueLoadAssets() => EnqueueLoadAssetsInDir("");

    /// <summary>
    /// Add <see cref="LoadAssetsInDirCommand"/> to queue.
    /// </summary>
    /// <param name="path">Directory path in this asset loader, assets from where to load.</param>
    public void EnqueueLoadAssetsInDir(string path) => TryAddCommand(new LoadAssetsInDirCommand(path, this));

    /// <summary>
    /// Add <see cref="ReloadAssetsInDirCommand"/> to queue.
    /// </summary>
    /// <param name="relativePath">Directory path in this asset loader, assets from where to reload.</param>
    public void EnqueueReloadAssetsInDir(string relativePath) => TryAddCommand(new ReloadAssetsInDirCommand(relativePath, this));

    /// <summary>
    /// Add <see cref="ReloadAssetCommand"/> to queue.
    /// </summary>
    /// <param name="path">File path in this asset loader of the asset to reload.</param>
    public void EnqueueReloadAsset(string path) => TryAddCommand(new ReloadAssetCommand(path, this));

    /// <summary>
    /// Add <see cref="RemoveAssetsInDirCommand"/> to queue.
    /// </summary>
    /// <param name="relativePath">Directory path in this asset loader.</param>
    public void EnqueueRemoveAssetsInDir(string relativePath) => TryAddCommand(new RemoveAssetsInDirCommand(relativePath, this));


    /// <summary>
    /// Load asset at the specified path in the cache synchronously, <b>replacing</b> already loaded assets. Useful for quickly loading fonts for the startup loading screen.
    /// </summary>
    public void LoadAsset(string path)
    {
        using AssetStream? assetStream = LoadAssetStream(path);
        LoadAssetFromAssetStream(path, assetStream);
    }

    /// <summary>
    /// Load asset at the <paramref name="path"/> asynchronously, <b>replacing</b> existing assets.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    /// <returns>Async task.</returns>
    public async Task LoadAssetAsync(string path)
    {
        using AssetStream? assetStream = await LoadAssetStreamAsync(path);
        LoadAssetFromAssetStream(path, assetStream);
    }


    private bool LoadAssetFromAssetStream(string path, AssetStream? assetStream)
    {
        if (assetStream is null) //When asset was deleted/renamed?
        {
            Log.Debug("{This}: Unloaded asset at {Path}.", this, path);
            RemoveFromCache(path);
            return false;
        }

        if (assetStream.Value.Type == AssetType.Ignore)
        {
            Log.Debug("{This}: Ignoring asset at {Path}.", this, path);
            return false;
        }


        AssetInfo assetInfo = assetStream.Value.ToInfo(MatchPath(path).ToArray(), path);
        AssetParser? parser = GetParser(assetInfo);
        if (parser is null)
        {
            Log.Warning("Could not find parser for the asset with type {Type} at path {Path}. Verify that parser is specified correctly and that asset has a supported format.", assetInfo.Type, path);
            return false;
        }

        object? asset = parser(assetInfo, Manager);
        if (asset is null) //failed to load (logged by parser) / loaded to somewhere else by parser
        {
            return false;
        }

        LoadIntoCache(path, asset);
        return true;
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
    protected async Task<AssetStream?> LoadAssetStreamAsync(string path)
    {
        path = path.Replace('\\', '/');
        string fullPath = Path.Join(DirectoryPath, path);
        if (!File.Exists(fullPath))
            return null;
        try
        {
            return new(new MemoryStream(await File.ReadAllBytesAsync(fullPath)), AssetsUtils.DetectTypeByPath(path));
        }
        catch (Exception exception)
        {
            Log.Error(exception, "An exception occured while trying to open a file:");
            return null;
        }
    }


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

        Log.Debug("{This}: Removed asset from the cache: {Path}", this, path);
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
                if (asset.StartsWith(dirPath))
                    Cache.Remove(asset);

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
        Log.Information("{This}: Unloaded {Count} assets", this, paths.Length);
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
}