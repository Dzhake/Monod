using Friflo.Engine.ECS.Systems;

namespace Monod.Utils.Extensions;

public static class SystemExtensions
{
    public static void RemoveAllSystems(this SystemGroup root)
    {
        // Convert child systems to array for safe removal
        foreach (var child in root.ChildSystems.ToArray())
        {
            if (child == null) continue;

            if (child is SystemGroup subgroup)
                subgroup.RemoveAllSystems();
            if (child is IDisposable disposable)
                disposable.Dispose();

            root.Remove(child);
        }
    }

    public static void RemoveAllStores(this SystemRoot systemRoot)
    {
        ArgumentNullException.ThrowIfNull(systemRoot, nameof(systemRoot));

        // Convert stores to array for safe removal
        foreach (var store in systemRoot.Stores.ToArray())
            systemRoot.RemoveStore(store);
    }

    public static void Destroy(this SystemRoot root)
    {
        root.RemoveAllSystems();
        root.RemoveAllStores();

        if (root is IDisposable disposable)
            disposable.Dispose();
    }
}
