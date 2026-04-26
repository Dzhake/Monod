namespace Monod.Utils.Extensions;

/// <summary>
/// Extensions for the <see cref="HashSet{T}"/>.
/// </summary>
public static class HashSetExtensions
{
    /// <summary>
    /// Adds the <paramref name="value"/> to the <paramref name="hashSet"/>, if <paramref name="hashSet"/> does not contain it, and removes the <paramref name="value"/> otherwise.
    /// </summary>
    /// <typeparam name="T">Type of the <paramref name="value"/>.</typeparam>
    /// <param name="hashSet">Hash set to use.</param>
    /// <param name="value">Value to toggle.</param>
    /// <returns>Whether value is contained in the <paramref name="hashSet"/> <b>after</b> the operation.</returns>
    public static bool ToggleValue<T>(this HashSet<T> hashSet, T value)
    {
        if (hashSet.Remove(value))
            return false;
        hashSet.Add(value);
        return true;
    }
}
