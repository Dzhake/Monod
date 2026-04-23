using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Monod.ModsModule.Exceptions;

public class ModListenerCountException : Exception
{

    public readonly string Issue;

    /// <inheritdoc/>
    public override string Message => Issue;

    /// <summary>
    /// Amount of <see cref="ModListener"/> found in the assembly.
    /// </summary>
    public int Count;

    public Assembly assembly;

    public ModListenerCountException(string issue, int count, Assembly assembly)
    {
        Issue = issue;
        Count = count;
        this.assembly = assembly;
    }

    [DoesNotReturn]
    public static void Throw(string issue, int count, Assembly assembly) => throw new ModListenerCountException(issue, count, assembly);
}
