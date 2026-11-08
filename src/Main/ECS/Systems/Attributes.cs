namespace Monod.ECS.Systems;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class MQueryAttribute<T1> : Attribute
{
    public MQueryAttribute(string? fieldName = null) => FieldName = fieldName;
    public string? FieldName { get; }
}