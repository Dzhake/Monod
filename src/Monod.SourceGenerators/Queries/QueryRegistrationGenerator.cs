using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Monod.SourceGenerators.Queries;

[Generator]
public sealed class QueryRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                "Monod.SourceGenerators.Queries.GenerateQueryRegistrationAttribute",
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax,
                transform: static (ctx, _) =>
                    (INamedTypeSymbol)ctx.TargetSymbol
            );

        var input = context.CompilationProvider.Combine(classes.Collect());

        context.RegisterSourceOutput(input, static (spc, data) =>
        {
            var compilation = data.Left;

            foreach (var symbol in data.Right)
            {
                Generate(spc, symbol, compilation);
            }
        });
    }

    private static void Generate(
    SourceProductionContext context,
    INamedTypeSymbol symbol,
    Compilation compilation)
    {
        var source = new StringBuilder();

        string? ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        if (ns != null)
        {
            source.AppendLine($"namespace {ns};");
            source.AppendLine();
        }

        string systemCtxType = "Monod.ECS.Systems.SystemCtx";

        source.AppendLine("public static partial class " + symbol.Name);
        source.AppendLine("{");
        source.AppendLine($"    public static void Register({systemCtxType} ctx)");
        source.AppendLine("    {");
        source.AppendLine("        var store = ctx.Store;");
        source.AppendLine();

        foreach (var field in symbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic == false)
                continue;

            if (field.IsConst || field.IsReadOnly)
                continue;

            if (field.Type is not INamedTypeSymbol keyType)
                continue;

            if (keyType.Name != "QueryKey" ||
                keyType.TypeArguments.Length != 1)
                continue;

            var queryType = keyType.TypeArguments[0];

            if (queryType is not INamedTypeSymbol archetype ||
                archetype.Name != "ArchetypeQuery")
                continue;

            bool isRelation = field.GetAttributes().Any(a =>
                a.AttributeClass?.Name == "QueryRelationAttribute");

            string fieldName = field.Name;

            if (archetype.TypeArguments.Length == 0)
            {
                // Tag query
                source.AppendLine(
                    $"        {fieldName} = ctx.RegisterQuery(" +
                    $"store.Query().AllTags(Tags.Get<{fieldName}>()));");
            }
            else
            {
                string type = archetype.TypeArguments[0]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (isRelation)
                {
                    source.AppendLine(
                        $"        {fieldName} = ctx.RegisterQuery(" +
                        $"store.QueryRelation<{type}>());");
                }
                else
                {
                    source.AppendLine(
                        $"        {fieldName} = ctx.RegisterQuery(" +
                        $"store.Query<{type}>());");
                }
            }
        }

        source.AppendLine("    }");
        source.AppendLine("}");

        context.AddSource(
            $"{symbol.Name}.QueryRegistration.g.cs",
            source.ToString());
    }
}
