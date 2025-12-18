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
    /// Throw a new <see cref="KeyNotFoundException"/>.
    /// </summary>
    /// <param name="key">Key that was not found in a given array/dictionary/etc.</param>
    [DoesNotReturn]
    public static void ThrowKeyNotFoundException(object key) => throw new KeyNotFoundException($"Key not found: {key}");

    /// <summary>
    /// Throw a new <see cref="InvalidOperationException"/> with the given <paramref name="message"/>.
    /// </summary>
    /// <param name="message">Message that is passed to <see cref="InvalidOperationException"/>.</param>
    [DoesNotReturn]
    public static void ThrowInvalidOperationException(string message) => throw new InvalidOperationException(message);
}