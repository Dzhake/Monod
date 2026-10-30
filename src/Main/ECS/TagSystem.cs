using Friflo.Engine.ECS.Systems;

namespace Monod.ECS;

/// <summary>
/// Small helper class for quickly creating systems that iterate entities with a specific component. Use <see cref="ArchetypeQuery.Entities"/> of the <see cref="Query"/> to iterate.
/// </summary>
/// <typeparam name="TTag">Type of that tag required in the query.</typeparam>
public class TagSystem<TTag> : BaseSystem where TTag : struct, ITag
{
    /// <summary>
    /// Query of entities with the <typeparamref name="TTag"/>.
    /// </summary>
    public ArchetypeQuery Query;

    ///<inheritdoc/>
    protected override void OnAddStore(EntityStore store)
    {
        Query = store.Query().AllTags(Tags.Get<TTag>());
        base.OnAddStore(store);
    }
}
