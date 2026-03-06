namespace Monod.AssetsModule.Commands;

/// <summary>
/// Remove all assets whose path starts with <paramref name="dir"/> from the <paramref name="loader"/>.
/// </summary>
/// <param name="dir">Directory path in the <paramref name="loader"/>.</param>
/// <param name="loader">Asset loader which executes this command.</param>
public sealed class RemoveAssetsInDirCommand(string dir, AssetLoader loader) : AssetLoaderCommand(loader)
{
    /// <summary>
    /// Directory path in the <see cref="AssetLoaderCommand.Loader"/>.
    /// </summary>
    public string Dir = dir;

    ///<inheritdoc/>
    public override string GetText() => $"{Loader} is removing assets at /{Dir}";

    ///<inheritdoc/>
    public override int TotalProgress => 1;

    ///<inheritdoc/>
    public override int CurrentProgress => IsFinished ? 1 : 0;

    ///<inheritdoc/>
    public async override Task Run()
    {
        Loader.RemoveDirFromCache(Dir);
        OnFinished();
    }
}