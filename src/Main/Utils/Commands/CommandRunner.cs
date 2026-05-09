using Monod.Utils.General;

namespace Monod.Utils.Commands;

/// <summary>
/// Class that runs a dynamic queue of <see cref="Command"/>s one by one, on a separate thread.
/// </summary>
/// <typeparam name="T">Type of the command this <see cref="CommandRunner{T}"/> runs.</typeparam>
public class CommandRunner<T> where T : Command
{
    /// <summary>
    /// Queue of commands to run. Commands are run when a new command is enqueued (by <see cref="AddCommand"/>) or when a command is finished (by <see cref="Command.Finish"/>). Access only with <see cref="CommandsLock"/>.
    /// </summary>
    protected Queue<T> Commands = new();

    /// <summary>
    /// Commands that is currently being run on another thread.
    /// </summary>
    public T? ActiveCommand;

    /// <summary>
    /// Whether this <see cref="CommandRunner{T}"/> currently doesn't execute any commands.
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


    /// <summary>
    /// Add the <paramref name="command"/> to the <see cref="Commands"/>, or run it, if <see cref="LoadingInactive"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="command">Command to add.</param>
    public virtual void AddCommand(T command)
    {
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
            T command = Commands.Dequeue();
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
    protected void RunCommand(T command)
    {
        ActiveCommand = command;
        MainThread.Add(Task.Run(command.Run));
    }
}
