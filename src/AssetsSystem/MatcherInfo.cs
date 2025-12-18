using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Monod.AssetsSystem;

/// <summary>
/// Struct that contains one entry in assets.json: a path matcher, and properties for assets matched by it.
/// </summary>
public struct MatcherInfo
{
    /// <summary>
    /// Path matcher, if matched by asset's path, then asset should use <see cref="Properties"/>.
    /// </summary>
    public Matcher PathMatcher;
    
    /// <summary>
    /// Properties that asset should use if it matches the <see cref="PathMatcher"/>.
    /// </summary>
    public Dictionary<int, object> Properties;

    /// <summary>
    /// Initialize a new <see cref="MatcherInfo"/> with the specific <see cref="PathMatcher"/> and <see cref="Properties"/>.
    /// </summary>
    /// <param name="pathMatcher">Path matcher, if matched by asset's path, then asset should use <see cref="Properties"/>.</param>
    /// <param name="properties">Properties that asset should use if it matches the <see cref="PathMatcher"/>.</param>
    public MatcherInfo(Matcher pathMatcher, Dictionary<int, object> properties)
    {
        PathMatcher = pathMatcher;
        Properties = properties;
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(PathMatcher, Properties);
    }
}