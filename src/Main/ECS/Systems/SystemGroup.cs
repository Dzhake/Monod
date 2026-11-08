using System.Collections;

namespace Monod.ECS.Systems;

public class SystemGroup : SystemBase, IEnumerable
{
    public readonly List<SystemBase> Systems;

    public SystemGroup(string name) : base(name)
    {
        Systems = new();
    }

    public override void OnCtxChanged(SystemCtx ctx)
    {
        foreach (var system in Systems)
        {
            if (!system.Enabled) continue;
            system.OnCtxChanged(ctx);
        }
    }

    public override void Update(SystemCtx ctx)
    {
        foreach (var system in Systems)
        {
            if (!system.Enabled) continue;
            system.Update(ctx);
        }
    }

    public void RemoveAllSystems()
    {
        foreach (var system in Systems)
        {
            (system as IDisposable)?.Dispose();
        }

        Systems.Clear();
    }

    public IEnumerator GetEnumerator()
    {
        return Systems.GetEnumerator();
    }

    public void Add(SystemBase system)
    {
        Systems.Add(system);
    }
}
