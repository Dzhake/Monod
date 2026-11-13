using System.Runtime.CompilerServices;

namespace Monod.Utils.Exceptions;

/// <summary>
/// Represents an unexcepted enum value.
/// </summary>
public class InvalidEnumValueException<TEnum> : Exception
{
    /// <summary>
    /// The value that is invalid.
    /// </summary>
    public TEnum Value;

    /// <summary>
    /// <see cref="CallerArgumentExpressionAttribute"/> for the <see cref="Value"/>. Used in the default message.
    /// </summary>
    public string Caller;

    public InvalidEnumValueException(TEnum value, [CallerArgumentExpression(nameof(value))] string? caller = null)
    {
        Value = value;
        Caller = caller;
    }

    public InvalidEnumValueException(string message, TEnum value) : base(message)
    {
        Value = value;
    }

    ///<inheritdoc/>
    public override string Message => string.IsNullOrEmpty(Caller) ? base.Message : $"{Caller} is not a valid value: {Value}";
}
