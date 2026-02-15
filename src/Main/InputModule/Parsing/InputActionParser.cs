using Monod.InputModule.InputActions;

namespace Monod.InputModule.Parsing;

/// <summary>
/// Provides functionality for parsing string representations of <see cref="InputAction"/> objects.
/// </summary>
/// <remarks>
/// Example syntax:
/// <code>
/// Or(Pressed(D1), And(Down(Ctrl), Pressed(D2)))
/// </code>
/// </remarks>
public static class InputActionParser
{
    public readonly static List<ActionParseError> Errors = new();

    /// <summary>
    /// Attempts to parse the specified text into an <see cref="InputAction"/>. Use <see cref="Errors"/> to access occurred errors.
    /// </summary>
    /// <param name="text">The input text to parse.</param>
    /// <returns>
    /// The parsed <see cref="InputAction"/> instance.
    /// If parsing fails, an <see cref="InvalidInputAction"/> is returned.
    /// </returns>
    public static InputAction TryParse(string text)
    {
        Errors.Clear();
        var span = text.AsSpan().Trim();
        var result = Parse(span, 0);
        return result;
    }

    /// <summary>
    /// Recursively parses a span of characters into an <see cref="InputAction"/>.
    /// </summary>
    /// <param name="s">Span to parse.</param>
    /// <param name="startingIndex">Starting index of the given span in some global span, to record error's indexes correctly.</param>
    private static InputAction Parse(ReadOnlySpan<char> s, int startingIndex)
    {
        s = s.Trim();
        if (s.IsEmpty) return Invalid("Empty input", startingIndex, 1, s);

        int open = s.IndexOf('(');
        if (open < 0) return Invalid("No opening bracket", startingIndex, s.Length, s);
        if (!s.EndsWith(")")) return Invalid("No closing bracket", startingIndex, s.Length, s);

        var name = s[..open].Trim();
        var inner = s[(open + 1)..^1];
        int innerStartIndex = startingIndex + open + 1;
        return ParseByName(s, startingIndex, name, inner, innerStartIndex);
    }

    /// <summary>
    /// Parse an <see cref="InputAction"/> based on it's name and the arguments.
    /// </summary>
    /// <param name="s">Span to parse.</param>
    /// <param name="startingIndex">Starting index of the given span in some global span, to record error's indexes correctly.</param>
    /// <param name="name">Name of the <see cref="InputAction"/>.</param>
    /// <param name="inner">Arguments to that <see cref="InputAction"/>'s ctor.</param>
    /// <param name="innerStartIndex">Starting index of the <paramref name="inner"/> part in some global span.</param>
    /// <returns>Parsed <see cref="InputAction"/> (possibly <see cref="InvalidInputAction"/> if parsing failed)</returns>
    private static InputAction ParseByName(ReadOnlySpan<char> s, int startingIndex, ReadOnlySpan<char> name, ReadOnlySpan<char> inner, int innerStartIndex)
    {
        return name switch
        {
            "Down" => ParseSingle(inner, innerStartIndex, s, static k => new DownAction(k)),
            "Pressed" => ParseSingle(inner, innerStartIndex, s, static k => new PressedAction(k)),
            "Up" => ParseSingle(inner, innerStartIndex, s, static k => new UpAction(k)),
            "Released" => ParseSingle(inner, innerStartIndex, s, static k => new ReleasedAction(k)),
            "Held" => ParseSingle(inner, innerStartIndex, s, static k => new HeldAction(k)),
            "Or" => ParseArray(inner, innerStartIndex, s, static a => new OrAction(a)),
            "And" => ParseArray(inner, innerStartIndex, s, static a => new AndAction(a)),
            _ => Invalid("Unknown action", startingIndex, s.Length, s),
        };
    }

    /// <summary>
    /// Parses a single-key action such as <c>Down(D1)</c>.
    /// </summary>
    private static InputAction ParseSingle(ReadOnlySpan<char> s, int innerStartIndex, ReadOnlySpan<char> fullSpan, Func<Key, InputAction> ctor)
    {
        s = s.Trim();
        if (!Enum.TryParse(s, out Key key)) return Invalid("Invalid key", innerStartIndex, s.Length, fullSpan);
        return ctor(key);
    }

    /// <summary>
    /// Parses an array action such as <c>Or(A, B)</c>.
    /// </summary>
    private static InputAction ParseArray(ReadOnlySpan<char> s, int innerStartIndex, ReadOnlySpan<char> fullSpan, Func<InputAction[], InputAction> ctor)
    {
        var list = new List<InputAction>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i <= s.Length - 1; i++)
        {
            if (s[i] == '(') depth++;
            if (s[i] == ')') depth--;

            if (s[i] == ',' && depth == 0)
            {
                var part = s[start..i].Trim();
                if (!part.IsEmpty)
                {
                    int localIndex = innerStartIndex + start;
                    list.Add(Parse(part, localIndex));
                }
                start = i + 1;
            }
        }

        var ending = s[start..].Trim();
        if (!ending.IsEmpty)
        {
            int localIndex = innerStartIndex + start;
            list.Add(Parse(ending, localIndex));
        }

        if (list.Count == 0) return Invalid("Empty array", innerStartIndex, s.Length, fullSpan);
        return ctor(list.ToArray());
    }

    /// <summary>
    /// Creates an <see cref="InvalidInputAction"/> and records a parsing error.
    /// </summary>
    private static InputAction Invalid(string message, int startIndex, int length, ReadOnlySpan<char> s)
    {
        Errors.Add(new ActionParseError(message, startIndex, length));
        return new InvalidInputAction(s.ToString());
    }
}

