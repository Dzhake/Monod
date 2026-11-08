namespace Monod.SourceGenerators.Queries;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateQueryRegistrationAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class QueryRelationAttribute : Attribute
{
}
