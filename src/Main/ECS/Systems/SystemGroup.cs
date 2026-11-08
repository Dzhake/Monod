using System.Collections;
using System.Diagnostics;

namespace Monod.ECS.Systems;

public class SystemGroup : SystemBase, IEnumerable
{
    public readonly List<SystemBase> ChildSystems;

    public SystemGroup(string name) : base(name)
    {
        ChildSystems = new();
    }

    public override void OnCtxChanged(SystemCtx ctx)
    {
        foreach (var system in ChildSystems)
        {
            if (!system.Enabled) continue;
            system.OnCtxChanged(ctx);
        }
    }

    public override void Update(SystemCtx ctx)
    {
        var selfPerf = ctx.MonitorPerf ? new PerfResource() : default;
        foreach (var system in ChildSystems)
        {
            if (!system.Enabled)
            {
                ClearPerfTicks(system, ctx);
                continue;
            }
            var childPerf = ctx.MonitorPerf ? new PerfResource() : default;
            system.Update(ctx);
            SetPerfTicks(system, childPerf);
        }
        SetPerfTicks(this, selfPerf);
    }

    public void RemoveAllSystems()
    {
        foreach (var system in ChildSystems)
        {
            (system as SystemGroup)?.RemoveAllSystems();
            (system as IDisposable)?.Dispose();
        }

        ChildSystems.Clear();
    }

    public IEnumerator GetEnumerator()
    {
        return ChildSystems.GetEnumerator();
    }

    public void Add(SystemBase system)
    {
        ChildSystems.Add(system);
    }

    protected static void SetPerfTicks(SystemBase system, in PerfResource start)
    {
        if (start.time == 0)
        {
            return;
        }
        long time = Stopwatch.GetTimestamp();
        long duration = time - start.time;
        long memory = PerfResource.GetAllocatedBytes() - start.memory;
        long[] history = system.Perf.history;
        int index = system.Perf.updateCount++ % history.Length;
        history[index] = duration;
        system.Perf.lastTicks = duration;
        system.Perf.sumTicks += duration;
        system.Perf.lastMemory = memory;
        system.Perf.sumMemory += memory;
    }

    protected void ClearPerfTicks(SystemBase system, SystemCtx ctx)
    {
        if (!ctx.MonitorPerf) return;
        system.Perf.lastTicks = -1;
        if (system is SystemGroup systemGroup)
        {
            foreach (var child in systemGroup.ChildSystems)
            {
                ClearPerfTicks(child, ctx);
            }
        }
    }
}
