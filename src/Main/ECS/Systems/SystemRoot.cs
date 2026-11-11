namespace Monod.ECS.Systems;

public class SystemRoot : SystemGroup
{
    public SystemCtx Ctx;

    public SystemRoot(string name) : base(name)
    {
    }

    public void Update()
    {
        var ctx = Ctx;
        if (!IsEnabled(ctx))
        {
            ClearPerfTicks(this, ctx);
            return;
        }
        Update(ctx);
        ctx.cb.Playback();
    }

    public void SetContext(SystemCtx ctx)
    {
        Ctx = ctx;
        OnCtxChanged(ctx);
    }
}
