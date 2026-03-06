namespace Monod.AssetsModule.Commands;

/// <summary>
/// Reload asset at the <paramref name="path"/> in the <paramref name="loader"/>.
/// </summary>
/// <param name="path">File path in the <paramref name="loader"/>.</param>
/// <param name="loader">Asset loader which executes this command.</param>
public sealed class ReloadAssetCommand(string path, AssetLoader loader) : AssetLoaderCommand(loader)
{
    /// <summary>
    /// Directory path in the <see cref="AssetLoaderCommand.Loader"/>.
    /// </summary>
    public string Path = path;

    ///<inheritdoc/>
    public override string GetText() => $"{Loader} is reloading asset at /{Path}";

    ///<inheritdoc/>
    public override int TotalProgress => 1;

    ///<inheritdoc/>
    public override int CurrentProgress => IsFinished ? 1 : 0;

    ///<inheritdoc/>
    public async override Task Run()
    {
        await Loader.LoadAssetAsync(Path);
        OnFinished();
    }
}