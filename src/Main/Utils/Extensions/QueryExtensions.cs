using Monod.ECS.Systems;

namespace Monod.Utils.Extensions;

public delegate void ForEachEntityM<T1>(ref T1 component1, Entity entity, SystemCtx ctx)
    where T1 : struct;

public static class QueryExtensions
{
    /// <summary>
    /// Executes the query for each matching entity, passing the <paramref name="ctx"/> to the callback.
    /// </summary>
    public static void ForEachEntityM<T1>(this ArchetypeQuery<T1> query, ForEachEntityM<T1> lambda, SystemCtx ctx) where T1 : struct
    {
        var localStore = query.Store;
        var nodes = localStore.nodes;
        foreach (var (chunk1, entities) in query.Chunks)
        {
            var span1 = chunk1.Span;
            var ids = entities.Ids;
            for (int n = 0; n < chunk1.Length; n++)
            {
                int id = ids[n];
                lambda(ref span1[n], new Entity(localStore, id, nodes[id].revision), ctx);
            }
        }
    }
}
