namespace Monod.InputModule.Parsing;

/// <summary>
/// Represents a single parsing error with position information.
/// </summary>
public sealed class ActionParseError
{
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public readonly string Message;

    /// <summary>
    /// Zero-based starting index of the error in the original input string.
    /// </summary>
    public readonly int StartIndex;

    /// <summary>
    /// Length of the erroneous segment.
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// Initialize a new instance of the <see cref="ActionParseError"/> class.
    /// </summary>
    /// <param name="message">Human-readable error message.</param>
    /// <param name="startIndex">Zero-based starting index of the error in the original input string.</param>
    /// <param name="length">Length of the erroneous segment.</param>
    public ActionParseError(string message, int startIndex, int length)
    {
        Message = message;
        StartIndex = startIndex;
        Length = length;
    }

    /// <summary>
    /// Returns a string representation of the error.
    /// </summary>
    public override string ToString()
    {
        return $"{StartIndex}-{StartIndex + Length}: {Message}";
    }
}