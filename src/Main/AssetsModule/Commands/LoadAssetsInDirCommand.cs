using Monod.Shared;

namespace Monod.AssetsModule.Commands;

public sealed class LoadAssetsInDirCommand(string dir, AssetLoader loader) : AssetLoaderCommand(loader)
{
    public string Dir = dir;

    ///<inheritdoc/>
    public override string GetText() => $"{Loader} is loading assets from /{Dir}";

    ///<inheritdoc/>
    public override int TotalProgress => TotalAssets;

    ///<inheritdoc/>
    public override int CurrentProgress => LoadedAssets;

    private int TotalAssets;
    private int LoadedAssets;

    ///<inheritdoc/>
    public async override Task Run()
    {
        var assetPaths = Loader.FilterPaths(Directory.GetFiles(Path.Join(Loader.DirectoryPath, Dir), "", SearchOption.AllDirectories).Select(item => Path.GetRelativePath(Loader.DirectoryPath, item).Replace('\\', '/'))).ToList();
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