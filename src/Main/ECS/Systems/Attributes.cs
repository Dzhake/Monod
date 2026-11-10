namespace Monod.ECS.Systems;

/// <summary>
/// When applied on a partial class, this attribute tells Monod's query generator to create a static field of type <see cref="QueryKey{T}"/> and to register the query (via the <see cref="SystemCtx.RegisterQuery{T}(T)"/>) in a static Register(<see cref="SystemCtx"/>) method of the class.
/// </summary>
/// <remarks>
/// Name of the field is equal to the attribute's type arguments joined by an underscore, or a name provided by the user in the constructor.
/// </remarks>
/// <typeparam name="T1">Type of the component/tag/relation to use in the query. Tags are applied via <see cref="ArchetypeQuery.AllTags"/>. One query can't have components and a relation, and there can be a max of 1 relation.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class MQueryAttribute<T1> : Attribute
{
    public MQueryAttribute(string? fieldName = null) => FieldName = fieldName;
    public string? FieldName { get; }
}