namespace Monod.ECS.Systems;

public abstract class SystemBase
{
    public string Name;
    public bool Enabled = true;
    public SystemPerf Perf;

    public SystemBase(string name)
    {
        Perf = new(10);
        Name = name;
    }

    public SystemBase()
    {
        Perf = new(10);
        Name = GetType().Name;
    }

    public virtual void OnCtxChanged(SystemCtx ctx) { }

    public abstract void Update(SystemCtx ctx);
}
