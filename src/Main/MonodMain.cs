global using Friflo.Engine.ECS;
global using Microsoft.Xna.Framework;
global using Serilog;
global using Math = System.Math;
using Monod.LogModule;

namespace Monod;

/// <summary>
/// Wrapper around various <see cref="Monod"/> modules.
/// </summary>
public static class MonodMain
{
    /// <summary>
    /// Call as early as possible. Initializes vital systems, such as <see cref="LogHelper"/>.
    /// </summary>
    public static void EarlyInitialize()
    {
        LogHelper.Initialize();
    }
}
