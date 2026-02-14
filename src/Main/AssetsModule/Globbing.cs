using Microsoft.Extensions.FileSystemGlobbing;

namespace Monod.AssetsModule;

/// <summary>
/// Util functions for Microsoft.Extensions.FileSystemGlobbing namespace.
/// </summary>
public class Globbing
{
    /// <summary>
    /// Create a new <see cref="Matcher"/>, and add patterns to it based on the <paramref name="definition"/>.
    /// </summary>
    /// <param name="definition">List of patterns separated by '|' (vertical bar, U+007C). If a pattern starts with '!' (exclamation mark, U+0021), then it's value excluding the '!' is added to 'exclude' patterns of the matcher.</param>
    /// <returns>New <see cref="Matcher"/> based on the <paramref name="definition"/>.</returns>
    public static Matcher MatcherFromString(string definition)
    {
        Matcher matcher = new();
        string[] matches = definition.Split('|');
        foreach (string match in matches)
        {
            if (match.StartsWith('!'))
                matcher.AddExclude(match[1..]);
            else
                matcher.AddInclude(match);
        }
        return matcher;
    }

    /// <summary>
    /// Create a new <see cref="Matcher"/>, and add patterns to it based on the <paramref name="definition"/>.
    /// </summary>
    /// <param name="definition">List of patterns separated by '|' (vertical bar, U+007C). If a pattern starts with '!' (exclamation mark, U+0021), then it's value excluding the '!' is added to 'exclude' patterns of the matcher.</param>
    /// <param name="relativePath">Relative path of the definition, that's added to every value in the definition in front of the definition.</param>
    /// <returns>New <see cref="Matcher"/> based on the <paramref name="definition"/>.</returns>
    public static Matcher MatcherFromString(string definition, string relativePath)
    {
        Matcher matcher = new();
        string[] matches = definition.Split('|');
        foreach (string match in matches)
        {
            if (match.StartsWith('!'))
                matcher.AddExclude(relativePath + match[1..]);
            else
                matcher.AddInclude(relativePath + match);
        }

        return matcher;
    }
}