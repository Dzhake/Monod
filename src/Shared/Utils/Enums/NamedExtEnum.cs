using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Monod.Utils.General;

/// <summary>
/// Extensible "enum"-like, where each value is associated with a <see cref="string"/> "name" for it. Provides methods for adding new values, and retrieving registered values by their name or names by associated values.
/// </summary>
public class NamedExtEnum
{
    /// <summary>
    /// List of registered names, where index is a value, and the value (object) in the list is value's name.
    /// </summary>
    private readonly List<string> Names;

    /// <summary>
    /// Dictionary of registered values, where key is value's name, and the value in the dictionary is the value.
    /// </summary>
    private readonly Dictionary<string, int> Values;

    /// <summary>
    /// Initialize a new instance of the <see cref="NamedExtEnum"/> with default values.
    /// </summary>
    public NamedExtEnum()
    {
        Names = new();
        Values = new(StringComparer.Ordinal);
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="NamedExtEnum"/> with the specified <paramref name="names"/>.
    /// </summary>
    /// <param name="names">List of names the enum should start with. Same as using <see cref="AddValue"/> for each value specified in the list, but more performant.</param>
    public NamedExtEnum(List<string> names)
    {
        Names = names;
        Values = new(StringComparer.Ordinal);
        for (int i = 0; i < names.Count; i++) Values.Add(names[i], i);
    }

    /// <summary>
    /// Add a new value with the specified <paramref name="name"/>, or return an existing one with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the new value. Must be unique for this value.</param>
    /// <returns>Value associated with the name.</returns>
    public int AddValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Values.TryGetValue(name, out int existingValue))
            return existingValue;

        int value = Names.Count;
        Names.Add(name);
        Values[name] = value;
        return value;
    }

    /// <summary>
    /// Get a value based on it's name.
    /// </summary>
    /// <param name="name">Name, that was used when registering the value.</param>
    /// <returns>Value associated with the specified name.</returns>
    public int GetValue(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Values.TryGetValue(name, out int value))
            return value;

        Guard.KeyNotFoundException(name);
        return 0; // unreachable
    }

    /// <summary>
    /// Get the value associated with the specified name in a error-safe way.
    /// </summary>
    /// <param name="name">The name of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified name, if the name is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>Whether the value was found.</returns>
    public bool TryGetValue(string name, out int value)
    {
        ArgumentNullException.ThrowIfNull(name);
        return Values.TryGetValue(name, out value);
    }

    /// <summary>
    /// Get the name associated with the specified value.
    /// </summary>
    /// <param name="value">The value of the name to get.</param>
    /// <returns>Name associated with the specified value.</returns>
    public string GetName(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, Names.Count);

        return Names[value];
    }

    /// <summary>
    /// Get the name associated with the specified value in a error-safe way.
    /// </summary>
    /// <param name="value">The value of the name to get.</param>
    /// <param name="name">Name associated with the specified value, or null if it was not found.</param>
    /// <returns>Whether the name was found.</returns>
    public bool TryGetName(int value, [NotNullWhen(true)] out string? name)
    {
        if ((uint)value < (uint)Names.Count)
        {
            name = Names[value];
            return true;
        }

        name = null;
        return false;
    }
}