namespace Monod.Utils.Commands;

/// <summary>
/// Command for <see cref="CommandRunner{T}"/> to run.
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Whether the command finished it's execution.
    /// </summary>
    public bool IsFinished { get; private set; }

    /// <summary>
    /// Total amount of progress this command has done and will do. Used for progress bars.
    /// </summary>
    public abstract int TotalProgress { get; }

    /// <summary>
    /// Current amount of progress this command has done. Used for progress bars.
    /// </summary>
    public abstract int CurrentProgress { get; }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <returns>Async task.</returns>
    public abstract Task Run();

    /// <summary>
    /// Get command's description. Can be used next to progress bars or in logs.
    /// </summary>
    /// <returns>Command's description.</returns>
    public abstract string GetText();

    /// <summary>
    /// Run when the command finishes. Marks the command as finished, and executes the next command.
    /// </summary>
    protected virtual void Finish()
    {
        IsFinished = true;

    }
}
