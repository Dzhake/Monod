using Monod.Graphics.ECS;

namespace Monod.ECS.Systems;

public abstract class SystemBase
{
    /// <summary>
    /// Name of the system, used for debugging.
    /// </summary>
    public string Name;

    /// <summary>
    /// Whether the <see cref="Update"/> should be called.
    /// </summary>
    public bool IsActive = true;

    /// <summary>
    /// Performance info about the system, updated on each <see cref="Update"/>.
    /// </summary>
    public SystemPerf Perf;

    /// <summary>
    /// <see cref="Stores"/>, in which this system should be updated.
    /// </summary>
    public Stores ActiveInStores;


    /// <summary>
    /// Create a new <see cref="SystemBase"/>.
    /// </summary>
    /// <param name="name">Name of system for debugging, uses the system's Type.Name by default.</param>
    /// <param name="activeInStores">Entity stores, in which the system should be updated, <see cref="Stores.All"/> by default.</param>
    protected SystemBase(string? name = null, Stores? activeInStores = null)
    {
        Perf = new(10);
        Name = name ?? GetType().Name;
        ActiveInStores = activeInStores ?? Stores.All;
    }

    /// <summary>
    /// Whether the <see cref="Update"/> should be called for this <paramref name="ctx"/>.
    /// </summary>
    public bool IsEnabled(SystemCtx ctx)
    {
        return IsActive && ((ActiveInStores & ctx.Stores) != Stores.None);
    }

    public virtual void OnCtxChanged(SystemCtx ctx) { }

    public abstract void Update(SystemCtx ctx);
}
