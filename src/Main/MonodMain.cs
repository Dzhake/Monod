using Monod.LogModule;

namespace Monod;

/// <summary>
/// Wrapper around various <see cref="Monod"/> modules.
/// </summary>
public static class MonodMain
{
    /// <summary>
    /// Call as early as possible. Initializes <see cref="LogHelper"/> and <see cref="OS"/>.
    /// </summary>
    public static void EarlyInitialize()
    {
        LogHelper.Initialize();
    }
}
