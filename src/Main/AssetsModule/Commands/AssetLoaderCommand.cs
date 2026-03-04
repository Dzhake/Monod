namespace Monod.AssetsModule.Commands;

public abstract class AssetLoaderCommand(AssetLoader loader)
{
    public AssetLoader Loader = loader;

    public bool IsFinished { get; protected set; }

    public abstract int TotalProgress { get; }
    public abstract int CurrentProgress { get; }

    public abstract Task Run();

    public abstract string GetText();

    protected void OnFinished()
    {
        IsFinished = true;
        Assets.IncrementFinishedCommandsCount();
        Loader.RunNextCommand();
    }
}
