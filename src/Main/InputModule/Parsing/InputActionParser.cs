using Chasm.Formatting;
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
    /// <summary>
    /// List of all errors for the last <see cref="TryParse"/>.
    /// </summary>
    public readonly static List<ActionParseError> Errors = new();

    /// <summary>
    /// Try to parse an <see cref="InputAction"/> from the <paramref name="text"/>.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>Parsed <see cref="InputAction"/>.</returns>
    public static InputAction TryParse(string text)
    {
        Errors.Clear();

        SpanParser parser = new SpanParser(text);
        InputAction result = ParseExpression(ref parser, text);
        parser.SkipWhitespaces();
        if (parser.CanRead() && result is not InvalidInputAction)
        {
            int start = parser.position;
            int length = parser.source.Length - start;
            return Invalid("Unexpected characters after expression", start, length, parser.source.Slice(start, length));
        }
        return result;
    }

    /// <summary>
    /// Recursively parses a single expression starting at the current parser position.
    /// </summary>
    private static InputAction ParseExpression(ref SpanParser parser, string originalText)
    {
        int exprStart = parser.position;
        parser.SkipWhitespaces();
        if (!parser.CanRead())
            return Invalid("Unexpected end of input", parser.position, 0, default);

        // Try to read an action name (ASCII letters)
        int nameStart = parser.position;
        ReadOnlySpan<char> nameSpan = ReadWhile(ref parser, static c => char.IsAsciiLetter(c));

        // No name found – unexpected character
        if (nameSpan.IsEmpty)
        {
            int errorStart = parser.position;
            SkipToNextSeparator(ref parser);
            return Invalid("Expected action name", errorStart, 1,
                parser.source[errorStart..parser.position]);
        }

        int nameLength = parser.position - nameStart;
        ReadOnlySpan<char> actionName = parser.source.Slice(nameStart, nameLength);

        // Unknown action name
        if (!IsKnownActionName(actionName))
        {
            int errorStart = nameStart;
            int errorEnd = parser.position;
            SkipToNextSeparator(ref parser);
            return Invalid("Unknown action name", errorStart, errorEnd - errorStart,
                parser.source[errorStart..parser.position]);
        }

        // Known name: now we expect '('
        parser.SkipWhitespaces();
        if (!parser.Skip('('))
        {
            int errorStart = nameStart;
            SkipToNextSeparator(ref parser);
            int errorEnd = parser.position;
            return Invalid($"Expected '(' after {actionName}", errorStart, errorEnd - errorStart,
                parser.source[errorStart..errorEnd]);
        }

        parser.SkipWhitespaces();
        // Delegate to argument parser based on action name
        return ParseActionArguments(ref parser, originalText, nameStart, nameLength);
    }

    /// <summary>
    /// Checks if a span matches any of the predefined action names.
    /// </summary>
    private static bool IsKnownActionName(ReadOnlySpan<char> name)
    {
        return name switch
        {
            "Or" or "And" or "Down" or "Up" or "Pressed" or "Released" or "Held" => true,
            _ => false
        };
    }

    /// <summary>
    /// Skips characters until the next top‑level comma or closing parenthesis.
    /// Handles nested parentheses to avoid stopping inside a subexpression.
    /// The parser is left positioned just before the separator (i.e., the next character
    /// will be either ',' or ')').
    /// </summary>
    private static void SkipToNextSeparator(ref SpanParser parser)
    {
        int depth = 0;
        while (parser.CanRead())
        {
            char c = parser.Peek();
            if (c == '(')
                depth++;
            else if (c == ')')
            {
                if (depth == 0)
                    break;
                depth--;
            }
            else if (c == ',' && depth == 0)
                break;

            parser.Read();
        }
    }

    private static InputAction ParseActionArguments(ref SpanParser parser, string originalText,
        int nameStart, int nameLength)
    {
        ReadOnlySpan<char> actionName = parser.source.Slice(nameStart, nameLength);

        if (actionName.SequenceEqual("Or".AsSpan()) || actionName.SequenceEqual("And".AsSpan()))
        {
            parser.SkipWhitespaces();
            if (parser.Peek() == ')')
            {
                parser.Skip(')');
                return actionName.SequenceEqual("Or".AsSpan())
                    ? new OrAction(Array.Empty<InputAction>())
                    : new AndAction(Array.Empty<InputAction>());
            }

            var actions = new List<InputAction>();
            while (true)
            {
                parser.SkipWhitespaces();
                // Recursively parse each argument – may return Invalid
                InputAction arg = ParseExpression(ref parser, originalText);
                actions.Add(arg);

                parser.SkipWhitespaces();
                if (parser.Skip(','))
                {
                    parser.SkipWhitespaces();
                }
                else if (parser.Skip(')'))
                {
                    break;
                }
                else
                {
                    // Unexpected character – consume until next separator and add an error
                    int errStart = parser.position;
                    SkipToNextSeparator(ref parser);
                    int errEnd = parser.position;
                    actions.Add(Invalid("Expected ',' or ')'", errStart, 1, parser.source.Slice(errStart, errEnd - errStart)));

                    // After consuming, we should be at a separator; try again
                    parser.SkipWhitespaces();
                    if (parser.Skip(','))
                    {
                        parser.SkipWhitespaces();
                    }
                    else if (parser.Skip(')'))
                    {
                        break;
                    }
                    else
                    {
                        // Still not a separator – give up and return the last error
                        return actions.Last();
                    }
                }
            }

            return actionName.SequenceEqual("Or".AsSpan())
                ? new OrAction(actions.ToArray())
                : new AndAction(actions.ToArray());
        }
        else
        {
            // Simple actions: expect a key name
            parser.SkipWhitespaces();
            int keyStart = parser.position;
            ReadOnlySpan<char> keySpan = ReadWhile(ref parser, static c => char.IsAsciiLetterOrDigit(c));
            if (keySpan.IsEmpty)
            {
                // No key name – skip to separator and return error covering from name start
                int errorStart = nameStart;
                SkipToNextSeparator(ref parser);
                int errorEnd = parser.position;
                return Invalid("Expected key name", errorStart, errorEnd - errorStart,
                    parser.source[errorStart..errorEnd]);
            }

            parser.SkipWhitespaces();
            if (!parser.Skip(')'))
            {
                // Missing ')' – skip to separator and return error covering from name start
                int errorStart = nameStart;
                SkipToNextSeparator(ref parser);
                int errorEnd = parser.position;
                return Invalid("Expected ')' after key", errorStart, errorEnd - errorStart,
                    parser.source[errorStart..errorEnd]);
            }

            if (!Enum.TryParse(keySpan, out Key key))
            {
                // Invalid key – skip to separator and return error covering from name start
                int errorStart = keyStart;
                SkipToNextSeparator(ref parser);
                int errorEnd = parser.position - 1;
                return Invalid("Invalid key name", errorStart, errorEnd - errorStart,
                    parser.source[nameStart..parser.position]);
            }

            if (actionName.SequenceEqual("Down".AsSpan())) return new DownAction(key);
            if (actionName.SequenceEqual("Up".AsSpan())) return new UpAction(key);
            if (actionName.SequenceEqual("Pressed".AsSpan())) return new PressedAction(key);
            if (actionName.SequenceEqual("Released".AsSpan())) return new ReleasedAction(key);
            if (actionName.SequenceEqual("Held".AsSpan())) return new HeldAction(key);

            // Should never reach here because we already checked known names
            int unknownStart = nameStart;
            SkipToNextSeparator(ref parser);
            int unknownEnd = keyStart - 1;
            return Invalid("Unknown action name", unknownStart, unknownEnd - unknownStart,
                parser.source[unknownStart..unknownEnd]);
        }
    }

    private static ReadOnlySpan<char> ReadWhile(ref SpanParser parser, Func<char, bool> predicate)
    {
        int start = parser.position;
        while (parser.CanRead() && predicate(parser.Peek()))
            parser.Read();
        return parser.source.Slice(start, parser.position - start);
    }

    /// <summary>
    /// Creates an <see cref="InvalidInputAction"/> and records a parsing error.
    /// </summary>
    private static InvalidInputAction Invalid(string message, int startIndex, int length, ReadOnlySpan<char> errorousSpan)
    {
        Errors.Add(new ActionParseError(message, startIndex, length));
        return new InvalidInputAction(errorousSpan.ToString());
    }
}

