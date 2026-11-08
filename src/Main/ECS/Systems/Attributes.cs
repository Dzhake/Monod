namespace Monod.ECS.Systems;


/// <summary>
/// Apply this attribute on a <see langword="static"/> <see langword="partial"/> class to make the source generator produce the "public static void Register(<see cref="SystemCtx"/>)" method, which creates a query for each field of type <see cref="QueryKey{T}"/> based on the provided context. Use other attributes in the <see cref="Monod.ECS.Systems"/> to better control the source generator. You can also declare a "RegisterManual(<see cref="SystemCtx"/>)" method in the class, that will be called by the source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateQueryRegistrationAttribute : Attribute;

/// <summary>
/// Indicates, that the source generator should use EntityStore.QueryRelation method to produce the query.
/// </summary>
/// <remarks>
/// See <see cref="GenerateQueryRegistrationAttribute"/> docs for common information about that source generator.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class QueryRelationAttribute : Attribute;

/// <summary>
/// Indicates, that the source generator should use <see cref="ArchetypeQuery.AllTags"/> when producing the query. Repeatable for multiple tags.</summary>
/// <remarks>
/// See <see cref="GenerateQueryRegistrationAttribute"/> docs for common information about that source generator.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class QueryTagAttribute<TTag> : Attribute;


/// <summary>
/// Indicates, that this field should be ignored by the source generator.
/// </summary>
/// <remarks>
/// See <see cref="GenerateQueryRegistrationAttribute"/> docs for common information about that source generator.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class QueryGenIgnoreAttribute : Attribute;