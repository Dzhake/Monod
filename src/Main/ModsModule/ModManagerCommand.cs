using Monod.Utils.Commands;

namespace Monod.ModsModule;

/// <summary>
/// A single <see cref="Command"/> the <see cref="ModManager"/> can execute.
/// </summary>
/// <param name="runner">Runner that executes this command.</param>
public abstract class ModManagerCommand(ModManagerCommandRunner runner) : Command
{
    /// <summary>
    /// Runner that executes this command.
    /// </summary>
    public ModManagerCommandRunner Runner = runner;

    ///<inheritdoc/>
    protected override void Finish()
    {
        base.Finish();
        Runner.RunNextCommand();
    }
}
