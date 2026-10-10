using System.Reflection;

namespace Monod.Utils.Extensions;

/// <summary>
/// Extensions for various <see cref="System.Reflection"/> classes.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Like <see cref="MethodInfo.CreateDelegate{T}()"/>, but on error instead of throwing returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the delegate to create.</typeparam>
    /// <param name="method"><see cref="MethodInfo"/> to bind to the delegate.</param>
    /// <returns>The delegate for this method.</returns>
    public static T? TryCreateDelegate<T>(this MethodInfo method) where T : Delegate
    {
        return Delegate.CreateDelegate(typeof(T), method, throwOnBindFailure: false) as T;
    }
}
