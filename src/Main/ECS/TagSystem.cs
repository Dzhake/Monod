using Friflo.Engine.ECS.Systems;

namespace Monod.ECS;

public class TagSystem<TTag> : BaseSystem where TTag : struct, ITag
{
    public ArchetypeQuery Query;

    protected override void OnAddStore(EntityStore store)
    {
        Query = store.Query().AllTags(Tags.Get<TTag>());
        base.OnAddStore(store);
    }
}
