using Monod.Utils.Commands;

namespace Monod.AssetsModule.Commands;

/// <summary>
/// A single command/task/instruction for <see cref="AssetLoader"/>. Commands are executed one by one (next is executed when <see cref="Finish"/> is called) to guarantee no issues.
/// </summary>
/// <param name="loader">Asset loader which executes this command.</param>
public abstract class AssetLoaderCommand(AssetLoader loader) : Command
{
    /// <summary>
    /// Asset loader which executes this command.
    /// </summary>
    public AssetLoader Loader = loader;

    ///<inheritdoc/>
    protected override void Finish()
    {
        base.Finish();
        Assets.IncrementFinishedCommandsCount();
        Loader.RunNextCommand();
    }
}
