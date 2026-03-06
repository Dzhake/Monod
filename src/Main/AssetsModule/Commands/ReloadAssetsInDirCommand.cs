using Monod.Shared;

namespace Monod.AssetsModule.Commands;

/// <summary>
/// Reload all assets from the <paramref name="dir"/> in the <paramref name="loader"/>.
/// </summary>
/// <param name="dir">Directory path in the <paramref name="loader"/>.</param>
/// <param name="loader">Asset loader which executes this command.</param>
public sealed class ReloadAssetsInDirCommand(string dir, AssetLoader loader) : AssetLoaderCommand(loader)
{
    /// <summary>
    /// Directory path in the <see cref="AssetLoaderCommand.Loader"/>.
    /// </summary>
    public string Dir = dir;

    ///<inheritdoc/>
    public override string GetText() => $"{Loader} is reloading assets at /{Dir}";

    ///<inheritdoc/>
    public override int TotalProgress => TotalAssets;

    ///<inheritdoc/>
    public override int CurrentProgress => LoadedAssets;

    private int TotalAssets;
    private int LoadedAssets;

    ///<inheritdoc/>
    public async override Task Run()
    {
        string fullDir = Path.Join(Loader.DirectoryPath, Dir);
        if (!Directory.Exists(fullDir))
        {
            OnFinished();
            return;
        }
        var assetPaths = Loader.FilterPaths(Directory.GetFiles(fullDir, "", SearchOption.AllDirectories).Select(item => Path.GetRelativePath(Loader.DirectoryPath, item).Replace('\\', '/'))).ToList();
        TotalAssets += assetPaths.Count;
        foreach (string assetPath in assetPaths)
            MainThread.Add(Task.Run(() => LoadAsset(assetPath)));
    }

    private async Task LoadAsset(string assetPath)
    {
        await Loader.LoadAssetAsync(assetPath);
        Interlocked.Add(ref LoadedAssets, 1);
        if (TotalAssets == LoadedAssets) OnFinished();
    }
}