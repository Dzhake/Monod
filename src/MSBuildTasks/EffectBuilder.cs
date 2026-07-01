using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Monod.MSBuild;

public static class EffectBuilder
{
    private static readonly Regex IncludeRegex = new(@"^\s*#include\s+""([^""]+)""");

    public static int BuildEffects(string rootDir, string outputDir)
    {
        Dictionary<string, EffectInfo> depsGraph = BuildReverseDependencyGraph(rootDir);
        HashSet<string> toCompile = new();
        foreach (KeyValuePair<string, EffectInfo> kvp in depsGraph)
        {
            DateTime lastModified = File.GetLastWriteTime(kvp.Key);
            foreach (string effect in kvp.Value.set)
                if (lastModified > File.GetLastWriteTime(Path.ChangeExtension(effect, "mgfx")))
                    toCompile.Add(effect);
        }
        foreach (string file in Directory.EnumerateFiles(rootDir, "*.fx", SearchOption.AllDirectories))
        {
            string normalizedFile = file.Replace('\\', '/');
            if (File.GetLastWriteTime(Path.ChangeExtension(normalizedFile, "mgfx")) < File.GetLastWriteTime(normalizedFile)) toCompile.Add(normalizedFile);
        }
        return OldEffectCompiler.Compile(toCompile.ToArray(), outputDir, rootDir);
    }

    private static Dictionary<string, EffectInfo> BuildReverseDependencyGraph(string rootDir)
    {
        var graph = new Dictionary<string, EffectInfo>(PathComparer);

        foreach (string file in Directory.EnumerateFiles(rootDir, "*.fx", SearchOption.AllDirectories).Concat(Directory.EnumerateFiles(rootDir, "*.fxh", SearchOption.AllDirectories)))
        {
            string fullFilePath = Path.GetFullPath(file.Replace('\\', '/'));
            string? fileDir = Path.GetDirectoryName(fullFilePath);
            if (fileDir is null) continue;
            string text = File.ReadAllText(fullFilePath);

            foreach (Match match in IncludeRegex.Matches(text))
            {
                string includePath = match.Groups[1].Value;
                string includedFile = Path.GetFullPath(Path.Combine(fileDir, includePath));

                if (!File.Exists(includedFile))
                    continue;

                if (!graph.TryGetValue(includedFile, out EffectInfo dependents))
                {
                    dependents = (new(PathComparer), File.GetLastWriteTime(includedFile));
                    graph[includedFile] = dependents;
                }

                dependents.set.Add(fullFilePath);
            }
        }

        return graph;
    }

    private static StringComparer PathComparer => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    private static StringComparison PathComparison => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}

public record struct EffectInfo(HashSet<string> set, DateTime lastModified)
{
    public static implicit operator (HashSet<string> set, DateTime lastModified)(EffectInfo value)
    {
        return (value.set, value.lastModified);
    }

    public static implicit operator EffectInfo((HashSet<string> set, DateTime lastModified) value)
    {
        return new EffectInfo(value.set, value.lastModified);
    }
}