namespace Monod.ECS.Systems;

public class SystemRoot : SystemGroup
{
    public SystemCtx Ctx;

    public SystemRoot(string name) : base(name)
    {
    }

    public void Update()
    {
        if (!Enabled)
        {
            ClearPerfTicks(this, Ctx);
            return;
        }
        Update(Ctx);
        Ctx.cb.Playback();
    }

    public void SetContext(SystemCtx ctx)
    {
        Ctx = ctx;
        OnCtxChanged(ctx);
    }
}
