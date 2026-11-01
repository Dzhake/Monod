using Chasm.Formatting;
using OneOf;
using OneOf.Types;

namespace Monod.Utils.General;

public static class CustomParsers
{
    /// <summary>
    /// Parses a list of ints separated by commas with the following special operations:<br/>
    /// Two numbers separated by - symbol are parsed as a range. For example, "2-5" turns into "2,3,4,5".<br/>
    /// Two numbers separated by * symbol are parsed as first number multiple times. For example, "2*5" turns into "2,2,2,2,2" (2 repeated 5 times). Order of the numbers matters!
    /// </summary>
    /// <param name="csv">Span to parse.</param>
    /// <returns>Parsed array.</returns>
    public static OneOf<int[], Error<string>> ParseAnimFrames(ReadOnlySpan<char> csv)
    {
        List<int> result = new();
        SpanParser parser = new(csv);
        while (parser.CanRead())
        {
            var frame1Span = parser.ReadAsciiDigits();
            if (!int.TryParse(frame1Span, out int frame1)) return new Error<string>($"Failed to parse symbol at position {parser.position} as int");

            if (parser.Skip(',') || !parser.CanRead())
            {
                result.Add(frame1);
                continue;
            }

            if (parser.Skip('-'))
            {
                while (true)
                {
                    var frame2Span = parser.ReadAsciiDigits();
                    if (!int.TryParse(frame2Span, out int frame2)) return new Error<string>($"Failed to parse symbol at position {parser.position} as int (expected second value of the range)");

                    int sign = Math.Sign(frame2 - frame1);
                    for (int j = frame1; j != frame2; j += sign)
                        result.Add(j);

                    if (!parser.Skip('-'))
                    {
                        result.Add(frame2);
                        break;
                    }

                    frame1 = frame2;
                }
            }
            else if (parser.Skip('*'))
            {
                var countSpan = parser.ReadAsciiDigits();
                if (!int.TryParse(countSpan, out int count)) return new Error<string>($"Failed to parse symbol at position {parser.position} as int (expected second value of the multiplication)");
                for (int i = 0; i < count; i++)
                    result.Add(frame1);
            }
            else
            {
                return new Error<string>($"Unexpected symbol at position {parser.position} (expected one of these symbols: , - * )");
            }

            if (parser.CanRead() && !parser.Skip(',')) return new Error<string>($"Expected comma at position {parser.position}");
        }

        return result.ToArray();
    }
}
