namespace Monod.AssetsModule;

/// <summary>
/// Type of resource program should aim for.
/// </summary>
public enum ResourcePriorityType
{
    /// <summary>
    /// Maximum performance, don't care about memory usage.
    /// </summary>
    Performance,

    /// <summary>
    /// Minimum memory usage, but probably lower performance.
    /// </summary>
    Memory,
}