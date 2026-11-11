using DerelictDimension.ECS.Drawing.Batching;
using Monod.Graphics.ECS;
using Monod.Shared.Exceptions;

namespace Monod.ECS.Systems;

/// <summary>
/// Context passed to the systems on system callbacks.
/// </summary>
public sealed class SystemCtx
{
    /// <summary>
    /// Store, in which the queries are executed.
    /// </summary>
    public EntityStore Store;

    /// <summary>
    /// Internal list of the registered queries.
    /// </summary>
    private List<ArchetypeQuery> Queries;

    /// <summary>
    /// <see cref="Store"/>'s command buffer.
    /// </summary>
    public CommandBuffer cb;

    /// <summary>
    /// Whether the systems should track and update performance history.
    /// </summary>
    public bool MonitorPerf;

    /// <summary>
    /// Batcher used by systems that draw entities to defer the rendering, to sort the draws by depth.
    /// </summary>
    public DrawBatcher drawBatcher;

    /// <summary>
    /// Id of the current <see cref="Store"/>.
    /// </summary>
    public Stores Stores;

    public SystemCtx(EntityStore store, Stores stores)
    {
        Store = store;
        Stores = stores;
        Queries = new();
        cb = store.GetCommandBuffer();
        cb.ReuseBuffer = true;
        drawBatcher = new();
    }

    /// <summary>
    /// Register a new query, making it available in the <see cref="GetQuery"/> via the returned key. Key is guaranteed to be valid for this specific <see cref="SystemCtx"/>, and currently, it's supposed to be persistent among all contexts, as long as the registration order and amounts stay the same.
    /// </summary>
    /// <typeparam name="T">Type of the query to register. This will be the type of query you'll receive when calling <see cref="GetQuery"/>.</typeparam>
    /// <param name="query">Query to register.</param>
    /// <returns>Key of the query, usable in the <see cref="GetQuery"/>.</returns>
    public QueryKey<T> RegisterQuery<T>(T query) where T : ArchetypeQuery
    {
        int index = Queries.Count;
        Queries.Add(query);
        return new(index);
    }

    /// <summary>
    /// Get a previously registered query.
    /// </summary>
    /// <typeparam name="T">Type of the query.</typeparam>
    /// <param name="key">Query's key.</param>
    /// <returns>The query.</returns>
    public T GetQuery<T>(QueryKey<T> key) where T : ArchetypeQuery
    {
        if (key.Value > Queries.Count) Guard.ArgumentException($"The provided query key is out of bounds. Key: {key.Value}, query type: {typeof(T)}, current queries count: {Queries.Count}");
        var query = Queries[key.Value];
        if (query is T castedQuery) return castedQuery;
        Guard.ArgumentException($"Failed to cast the query with key {key.Value} to type {typeof(T)}");
        return default!;
    }
}
