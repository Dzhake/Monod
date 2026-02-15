using System;

namespace Monod.Shared.Exceptions;

/// <summary>
/// The exception that is thrown when received type of some object was not what expected.
/// </summary>
public class TypeMismatchException : Exception
{
    /// <summary>
    /// Expected type of the asset.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Type if the asset which was received.
    /// </summary>
    public Type ReceivedType { get; }

    /// <inheritdoc/>
    public override string Message => $"Type mismatch: expected {ExpectedType}, received {ReceivedType}";

    /// <summary>
    ///   <para>Initialize a new instance of the <see cref="TypeMismatchException"/> class.</para>
    /// </summary>
    /// <param name="expectedType">Type that was expected.</param>
    /// <param name="receivedType">Type that was received.</param>
    public TypeMismatchException(Type expectedType, Type receivedType)
    {
        ArgumentNullException.ThrowIfNull(expectedType);
        ArgumentNullException.ThrowIfNull(receivedType);
        ExpectedType = expectedType;
        ReceivedType = receivedType;
    }
}