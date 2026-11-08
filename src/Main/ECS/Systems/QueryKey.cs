namespace Monod.ECS.Systems;

public record struct QueryKey<T> where T : ArchetypeQuery
{
    public int Value;

    public QueryKey(int value)
    {
        Value = value;
    }
}
