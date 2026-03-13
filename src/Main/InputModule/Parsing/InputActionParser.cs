using Chasm.Formatting;
using JetBrains.Annotations;
using Monod.InputModule.InputActions;

namespace Monod.InputModule.Parsing;

/// <summary>
/// Parses textual representations of <see cref="InputAction"/>s.
/// </summary>
public static class InputActionParser
{
    /// <summary>
    /// Source string tat is currently being parsed.
    /// </summary>
    private static string Source = string.Empty;

    /// <summary>
    /// Parse <paramref name="text"/> into an <see cref="InputAction"/>. If input is invalid returns <see cref="InvalidInputAction"/>.
    /// </summary>
    public static InputAction Parse(string text)
    {
        Source = text;
        if (string.IsNullOrEmpty(text)) return new InvalidInputAction(string.Empty, new("Empty input", 0, 1));

        var parser = new SpanParser(text);
        var result = ParseAction(ref parser);
        Source = string.Empty;
        return result;
    }

    private static InputAction ParseAction(ref SpanParser parser)
    {
        parser.SkipWhitespaces();
        int nameStart = parser.position;

        // Read action name (letters only?)
        var name = parser.ReadAsciiLetters();

        if (name.Length == 0)
            return Error("Expected action name", parser.position, 1);

        parser.SkipWhitespaces();
        if (!parser.Skip('('))
            return Error("Expected '(' after action name", parser.position, 1);

        if (IsKeyBasedAction(name))
            return ParseKeyBasedAction(ref parser, name);

        if (IsArrayBasedAction(name))
            return ParseArrayBasedAction(ref parser, name);

        return Error("Unknown action name", nameStart, name.Length);
    }

    private static bool IsKeyBasedAction(ReadOnlySpan<char> name)
    {
        return name is "Down" or "Up" or "Held" or "Pressed" or "Released";
    }

    private static InputAction ParseKeyBasedAction(ref SpanParser parser, ReadOnlySpan<char> name)
    {
        parser.SkipWhitespaces();
        int keyStart = parser.position;
        var keySpan = parser.ReadUntil(')');

        if (keySpan.Length == 0)
            return Error("Expected key", keyStart, 1);

        if (!parser.Skip(')'))
            return Error("Expected ')' after key", parser.position, 1);

        string keyStr = keySpan.ToString();
        if (!Enum.TryParse<Key>(keyStr, out var key))
            return Error("Invalid key name", keyStart, keySpan.Length);

        InputAction? action = CreateKeyBasedAction(name, key);
        if (action is null)
            return Error("Could not create key based action (Unknown action name)", keyStart, parser.position - keyStart);
        return action;
    }

    private static InputAction? CreateKeyBasedAction(ReadOnlySpan<char> name, Key key)
    {
        if (name is "Down") return new DownAction(key);
        if (name is "Up") return new UpAction(key);
        if (name is "Held") return new HeldAction(key);
        if (name is "Pressed") return new PressedAction(key);
        if (name is "Released") return new ReleasedAction(key);
        return null;
    }

    private static bool IsArrayBasedAction(ReadOnlySpan<char> name)
    {
        return name is "Or" or "And";
    }

    private static InputAction ParseArrayBasedAction(ref SpanParser parser, ReadOnlySpan<char> nameSpan)
    {
        var items = new List<InputAction>();
        parser.SkipWhitespaces();

        int actionStart = parser.position;

        // Handle empty list like Or()
        if (parser.Skip(')'))
            return CreateArrayBasedAction(actionStart, nameSpan, items);

        while (true)
        {
            parser.SkipWhitespaces();
            int itemStart = parser.position;
            if (!parser.CanRead())
            {
                return Error("Expected action name", itemStart, 1);
            }

            var action = ParseAction(ref parser);
            if (action is InvalidInputAction)
                return action;

            items.Add(action);

            parser.SkipWhitespaces();
            if (parser.Skip(','))
                continue;

            if (parser.Skip(')'))
                break;

            return Error("Expected ',' or ')'", parser.position, 1);
        }

        return CreateArrayBasedAction(actionStart, nameSpan, items);
    }

    private static InputAction CreateArrayBasedAction(int actionStart, ReadOnlySpan<char> name, List<InputAction> items)
    {
        if (name is "Or")
            return new OrAction(items.ToArray());
        else if (name is "And")
            return new AndAction(items.ToArray());
        else return Error("Couldn't create array based action (Unknown action name)", actionStart, actionStart + name.Length);
    }

    [MustUseReturnValue]
    private static InvalidInputAction Error(string message, int startIndex, int length)
    {
        return new(Source, new(message, startIndex, length));
    }
}
