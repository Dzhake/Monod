namespace Monod.Shared;

public static class MonodSettings
{
    /// <summary>
    /// Whether the program should do operations related to hot reloading assets/.dlls etc. at cost of performance and memory. This does not include <b>fully</b> reloading mods.
    /// </summary>
    public static bool HotReload = true;
}
