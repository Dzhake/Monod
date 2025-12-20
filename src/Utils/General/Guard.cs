using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Monod.Utils.General;

/// <summary>
/// Class for methods that only throw exception, for performance.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throw a new <see cref="System.Collections.Generic.KeyNotFoundException"/>.
    /// </summary>
    /// <param name="key">Key that was not found in a given array/dictionary/etc.</param>
    [DoesNotReturn]
    public static void KeyNotFoundException(object key) => throw new KeyNotFoundException($"Key not found: '{key}'");

    /// <summary>
    /// Throw a new <see cref="ArgumentException"/> with message: "Key already exists: '{key}'".
    /// </summary>
    /// <param name="key">Key that already exists in a given array/dictionary/etc.</param>
    public static void DuplicateKeyException(object key) => throw new ArgumentException($"Key already exists: '{key}'.");

    /// <summary>
    /// Throw a new <see cref="System.InvalidOperationException"/> with the given <paramref name="message"/>.
    /// </summary>
    /// <param name="message">Message that is passed to <see cref="System.InvalidOperationException"/>.</param>
    [DoesNotReturn]
    public static void InvalidOperationException(string message) => throw new InvalidOperationException(message);
}