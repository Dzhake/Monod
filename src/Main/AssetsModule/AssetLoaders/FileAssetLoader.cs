namespace Monod.AssetsModule.AssetLoaders;

/// <summary>
/// Implementation of <see cref="AssetLoader"/> loading assets from the specified <see cref="DirectoryPath"/>.
/// </summary>
/*public sealed class FileAssetLoader : AssetLoader
{

    
    /// <summary>
    /// <see cref="FileSystemWatcher"/>, used to detect changes in files that are stored in <see cref="DirectoryPath"/>.
    /// </summary>
    private FileSystemWatcher? Watcher;

    /// <summary>
    /// Initialize a new <see cref="FileAssetLoader"/> with the specified <see cref="DirectoryPath"/>.
    /// </summary>
    /// <param name="directoryPath">The full path to the directory that this asset loader loads assets from.</param>
    public FileAssetLoader(string directoryPath) : base(directoryPath)
    {
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
        Watcher.Deleted += OnFileChanged;
        Watcher.Renamed += OnFileRenamed;

        Watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Event for <see cref="Watcher"/> to call when a file's content changes or the file is created.
    /// </summary>
    /// <param name="sender">Event sender, usually <see cref="Watcher"/>.</param>
    /// <param name="e">Arguments of the event.</param>
    private void OnFileChanged(object sender, FileSystemEventArgs e) => OnFileChanged(e.FullPath);

    private void OnFileChanged(string path)
    {
        string relPath = Path.GetRelativePath(DirectoryPath, path);
        if (Assets.ReloadQueue?.Contains((this, relPath)) ?? false)
            return;
        ReloadAsset(relPath);
    }

    /// <summary>
    /// Event for <see cref="Watcher"/> to call when a file is renamed.
    /// </summary>
    /// <param name="sender">Event sender, usually <see cref="Watcher"/>.</param>
    /// <param name="e">Arguments of the event.</param>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        OnFileChanged(e.OldFullPath);
        OnFileChanged(e.FullPath);
    }

    /// <summary>
    /// Adds asset at the specified path to the reload queue.
    /// </summary>
    /// <param name="path">Path of the asset in this <see cref="AssetLoader"/>.</param>
    private void ReloadAsset(string path)
    {
        try
        {
            Assets.LoadingInfoLock.EnterWriteLock();
            Assets.ReloadQueue?.Add((this, path));
            Assets.Reloading = true;
        }
        finally
        {
            Assets.LoadingInfoLock.ExitWriteLock();
        }
    }
}*/