using Monod.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace Monod.AssetsModule;

public static partial class EffectBuilder
{
    private static readonly Regex IncludeRegex = GetIncludeRegex();

    public static void BuildEffectsOnDirChanged(string relativePath, string rootDir)
    {
        string fullDirPath = Path.GetFullPath(Path.Combine(rootDir, relativePath));
        if (!Directory.Exists(fullDirPath)) return;

        var changedFiles = Directory.EnumerateFiles(fullDirPath, "*.fx", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(fullDirPath, "*.fxh", SearchOption.AllDirectories));

        BuildEffectsOnFilesChanged(changedFiles, rootDir);
    }

    public static void BuildEffectsOnFxhChanged(string filePath, string rootDir)
    {
        string fullFxhPath = Path.GetFullPath(Path.Combine(rootDir, filePath));
        if (!File.Exists(fullFxhPath)) return;
        BuildEffectsOnFilesChanged([fullFxhPath], rootDir);
    }

    private static void BuildEffectsOnFilesChanged(IEnumerable<string> changedFiles, string rootDir)
    {
        var depsGraph = BuildReverseDependencyGraph(rootDir);
        var affectedFiles = new HashSet<string>(PathComparer);
        BuildEffects(GetDependents(changedFiles, depsGraph).Where(f => string.Equals(Path.GetExtension(f), ".fx", PathComparison)).ToArray(), rootDir);
    }

    public static void BuildEffects(string[] effectFiles, string rootDir)
    {
        EffectCompiler.Compile(effectFiles, rootDir, rootDir);
    }

    public static void BuildEffect(string effectFile, string rootDir) => EffectCompiler.Compile([effectFile], rootDir, rootDir);

    private static Dictionary<string, HashSet<string>> BuildReverseDependencyGraph(string rootDir)
    {
        var graph = new Dictionary<string, HashSet<string>>(PathComparer);

        foreach (string file in Directory.EnumerateFiles(rootDir, "*.fx", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(rootDir, "*.fxh", SearchOption.AllDirectories)))
        {
            string fullFilePath = Path.GetFullPath(file);
            string? fileDir = Path.GetDirectoryName(fullFilePath);
            if (fileDir is null) continue;
            string text = File.ReadAllText(fullFilePath);

            foreach (Match match in IncludeRegex.Matches(text))
            {
                string includePath = match.Groups[1].Value;
                string includedFile = Path.GetFullPath(Path.Combine(fileDir, includePath));

                if (!File.Exists(includedFile))
                    continue;

                if (!graph.TryGetValue(includedFile, out HashSet<string>? dependents))
                {
                    dependents = new HashSet<string>(PathComparer);
                    graph[includedFile] = dependents;
                }

                dependents.Add(fullFilePath);
            }
        }

        return graph;
    }

    private static HashSet<string> GetDependents(IEnumerable<string> changedFiles, Dictionary<string, HashSet<string>> reverseDependencies)
    {
        var result = new HashSet<string>(PathComparer);
        var queue = new Queue<string>();

        foreach (string file in changedFiles)
        {
            result.Add(file);
            queue.Enqueue(file);
        }

        int i = 0;
        while (queue.Count > 0)
        {
            string file = queue.Dequeue();

            if (!reverseDependencies.TryGetValue(file, out HashSet<string>? dependents))
                continue;

            foreach (string dependent in dependents)
            {
                if (!result.Add(dependent))
                    continue;

                queue.Enqueue(dependent);
            }
            i++;
            if (i > 100000) break;
        }
        if (i > 100000) Guard.Exception("Effect Builder got 100 000 files as input, clearly this is not normal! Check that you don't have a circular dependency in your #include."); //circular dependency shouldn't actually matter? uh who knows i don't want the game to just freeze

        return result;
    }

    private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    private static StringComparison PathComparison => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    [GeneratedRegex(@"^\s*#include\s+""([^""]+)""", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex GetIncludeRegex();
}