using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Monod.SourceGenerators.Queries;

[Generator]
public sealed class QueryRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Stage 1 (cached per attributed class): everything that needs symbols or
        // the compilation happens HERE, and the output is pure data — records with
        // only strings/bools/immutable arrays. That's what keeps the pipeline
        // incremental: editing an unrelated file triggers zero work.
        var models = context.SyntaxProvider.ForAttributeWithMetadataName(
            WellKnown.GenerateQueryRegistrationAttribute,
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, ct) =>
                QueryClassModel.Create((INamedTypeSymbol)ctx.TargetSymbol, ctx.SemanticModel.Compilation, ct));

        // No CompilationProvider.Combine — it re-runs the output stage on every
        // keystroke project-wide. All compilation access happens in the transform.
        context.RegisterSourceOutput(models.Collect(), static (spc, models) =>
        {
            // Distinct guards against the attribute being applied to two partial parts.
            foreach (var model in models.Distinct())
                model.Execute(spc);
        });
    }
}

/// <summary>
/// The entire engine contract, in one place. This is the only coupling between
/// the generator and the engine, and every entry is validated against the
/// consumer's compilation — a mismatch produces a diagnostic, never broken
/// generated code.
/// </summary>
internal static class WellKnown
{
    // Attributes — adjust if you move them into the engine.
    public const string GenerateQueryRegistrationAttribute = "Monod.ECS.Systems.GenerateQueryRegistrationAttribute";
    public const string QueryRelationAttribute = "Monod.ECS.Systems.QueryRelationAttribute";
    public const string QueryTagAttribute = "Monod.ECS.Systems.QueryTagAttribute`1";
    public const string QueryGenIgnoreAttribute = "Monod.ECS.Systems.QueryGenIgnoreAttribute";

    // Engine types — adjust to your real namespaces.
    public const string SystemCtxType = "Monod.ECS.Systems.SystemCtx";
    public const string QueryKeyType = "Monod.ECS.Systems.QueryKey`1";
    public const string TagsType = "Friflo.Engine.ECS.Tags";
    public const string ArchetypeQueryType = "Friflo.Engine.ECS.ArchetypeQuery";
}

internal static class Descriptors
{
    private const string Category = "MonodQueryGen";

    public static readonly DiagnosticDescriptor EngineTypeMissing = Make("MGQ001",
        "Engine type not found",
        "The engine type '{0}' could not be resolved. Update the WellKnown constants in the query source generator to the engine's actual namespaces.");

    public static readonly DiagnosticDescriptor AttributeMetadataNameMismatch = Make("MGQ002",
        "Query attribute found under an unexpected name",
        "Attribute '{0}' was found as '{1}', which does not match the generator's WellKnown constant. Move the attribute or update the constant.");

    public static readonly DiagnosticDescriptor ClassMustBePartial = Make("MGQ010",
        "Class must be partial",
        "Class '{0}' must be declared partial so the generator can add the Register method.");

    public static readonly DiagnosticDescriptor ClassMustNotBeGeneric = Make("MGQ011",
        "Class must not be generic",
        "Class '{0}' cannot be generic: its static fields would be per-instantiation and registration would break.");

    public static readonly DiagnosticDescriptor FieldMustBeStatic = Make("MGQ012",
        "Query field must be static",
        "Field '{0}' must be static.");

    public static readonly DiagnosticDescriptor FieldMustNotBeReadOnly = Make("MGQ013",
        "Query field must not be readonly",
        "Field '{0}' cannot be readonly because Register assigns it. Remove readonly or add [QueryGenIgnore].");

    public static readonly DiagnosticDescriptor QueryMustBeArchetypeQuery = Make("MGQ014",
        "QueryKey must wrap an engine ArchetypeQuery",
        "Field '{0}' has type '{1}'; the generator only handles QueryKey<ArchetypeQuery<...>> fields.");

    public static readonly DiagnosticDescriptor RelationNeedsSingleTypeArgument = Make("MGQ015",
        "Relation queries take exactly one type argument",
        "Field '{0}' is marked [QueryRelation] and must be QueryKey<ArchetypeQuery<TRelation>> with no [QueryTag]s.");

    public static readonly DiagnosticDescriptor QueryNeedsComponentsOrTags = Make("MGQ016",
        "Query has no components or tags",
        "Field '{0}' declares an empty query; add type arguments or at least one [QueryTag<T>].");

    public static readonly DiagnosticDescriptor RegisterIsGenerated = Make("MGQ017",
        "Register is generated",
        "This Register method is generated by QueryRegistrationGenerator. Delete it and move custom code into 'static void RegisterManual(SystemCtx ctx)', which the generator calls automatically.");

    public static readonly DiagnosticDescriptor BadRegisterManualSignature = Make("MGQ018",
        "Invalid RegisterManual signature",
        "RegisterManual must be 'static void RegisterManual({1} ctx)'; found '{0}'.");

    public static readonly DiagnosticDescriptor AmbiguousRegisterManual = Make("MGQ019",
        "Ambiguous RegisterManual",
        "Multiple valid RegisterManual overloads were found; keep exactly one.");

    public static readonly DiagnosticDescriptor BadQueryTagUsage = Make("MGQ020",
        "Invalid [QueryTag] usage",
        "The [QueryTag] attribute on field '{0}' must specify its type argument: [QueryTag<MyTag>].");

    public static readonly DiagnosticDescriptor EngineApiMissing = Make("MGQ021",
        "Engine API missing",
        "Engine member '{0}' could not be found; the engine and the generator are out of sync.");

    public static DiagnosticDescriptor? Find(string id) => id switch
    {
        "MGQ001" => EngineTypeMissing,
        "MGQ002" => AttributeMetadataNameMismatch,
        "MGQ010" => ClassMustBePartial,
        "MGQ011" => ClassMustNotBeGeneric,
        "MGQ012" => FieldMustBeStatic,
        "MGQ013" => FieldMustNotBeReadOnly,
        "MGQ014" => QueryMustBeArchetypeQuery,
        "MGQ015" => RelationNeedsSingleTypeArgument,
        "MGQ016" => QueryNeedsComponentsOrTags,
        "MGQ017" => RegisterIsGenerated,
        "MGQ018" => BadRegisterManualSignature,
        "MGQ019" => AmbiguousRegisterManual,
        "MGQ020" => BadQueryTagUsage,
        "MGQ021" => EngineApiMissing,
        _ => null,
    };

    private static DiagnosticDescriptor Make(string id, string title, string message) =>
        new(id, title, message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}

// ---------------------------------------------------------------------------
// Equatable, pure-data models. Everything Roslyn-flavored is converted to
// strings/bools here, then never touched again.
// ---------------------------------------------------------------------------

internal sealed record QueryClassModel(
    string? Namespace,
    EquatableArray<NestingType> Nesting,     // outermost first, excludes the class itself
    string ClassName,
    bool IsStatic,
    string? SystemCtxFqn,                    // 'global::Monod.ECS.Systems.SystemCtx'
    string? TagsFqn,                         // 'global::Monod.ECS.Queries.Tags'
    bool CallRegisterManual,
    bool EmitRegister,                       // false → report diagnostics only
    EquatableArray<QueryFieldModel> Fields,
    EquatableArray<DiagnosticInfo> Diagnostics)
{
    public static QueryClassModel Create(
        INamedTypeSymbol symbol,
        Compilation compilation,
        CancellationToken ct)
    {
        var diagnostics = new List<DiagnosticInfo>();
        var classLocation = LocationInfo.From(symbol.Locations.FirstOrDefault()) ?? LocationInfo.Empty;

        // ---- resolve the engine contract from the CONSUMER's compilation ----
        var systemCtx = compilation.GetTypeByMetadataName(WellKnown.SystemCtxType);
        var queryKey = compilation.GetTypeByMetadataName(WellKnown.QueryKeyType);
        var tagsType = compilation.GetTypeByMetadataName(WellKnown.TagsType);

        if (systemCtx is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.SystemCtxType);
        if (queryKey is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.QueryKeyType);
        if (tagsType is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.TagsType);

        bool canEmit = systemCtx is not null && queryKey is not null;

        // ---- class-level gates ------------------------------------------------
        if (symbol.IsGenericType)
        {
            Add(diagnostics, Descriptors.ClassMustNotBeGeneric, classLocation, symbol.Name);
            canEmit = false;
        }

        if (!IsDeclaredPartial(symbol, ct))
        {
            Add(diagnostics, Descriptors.ClassMustBePartial, classLocation, symbol.Name);
            canEmit = false;
        }

        // A hand-written Register would collide with the generated one. (Generated
        // trees are not visible to this generator, so any hit is user code.)
        var userRegister = symbol.GetMembers("Register")
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary)
            .ToImmutableArray();

        if (userRegister.Length > 0)
        {
            Add(diagnostics, Descriptors.RegisterIsGenerated,
                LocationInfo.From(userRegister[0].Locations.FirstOrDefault()) ?? LocationInfo.Empty);
            canEmit = false;
        }

        // ---- RegisterManual: visible here because TargetSymbol is the MERGED
        //      partial-class symbol (all parts, all files). ---------------------
        bool callRegisterManual = false;
        var validManuals = new List<IMethodSymbol>();

        foreach (var method in symbol.GetMembers("RegisterManual").OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary) continue;

            var location = LocationInfo.From(method.Locations.FirstOrDefault()) ?? LocationInfo.Empty;

            bool signatureOk = method.IsStatic
                && method.ReturnsVoid
                && method.Parameters.Length == 1
                && systemCtx is not null
                && ParameterAccepts(method.Parameters[0].Type, systemCtx);

            if (!signatureOk)
            {
                Add(diagnostics, Descriptors.BadRegisterManualSignature, location,
                    method.ToDisplayString(),
                    systemCtx?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? WellKnown.SystemCtxType);
            }
            else
            {
                validManuals.Add(method);
            }
        }

        if (validManuals.Count > 1)
        {
            Add(diagnostics, Descriptors.AmbiguousRegisterManual, classLocation);
        }
        else
        {
            callRegisterManual = validManuals.Count == 1;
        }

        // ---- engine API presence (class-level) --------------------------------
        INamedTypeSymbol? storeType = null;

        if (systemCtx is not null)
        {
            var storeSymbol = systemCtx.GetMembers("Store").OfType<IFieldSymbol>().FirstOrDefault();
            if (storeSymbol is null || storeSymbol.Type is not INamedTypeSymbol store)
            {
                Add(diagnostics, Descriptors.EngineApiMissing, classLocation, "SystemCtx.Store");
                canEmit = false;
            }
            else
            {
                storeType = store;
                if (!HasMethod(systemCtx, "RegisterQuery", 1))
                {
                    Add(diagnostics, Descriptors.EngineApiMissing, classLocation, "SystemCtx.RegisterQuery");
                    canEmit = false;
                }
            }
        }

        // ---- fields -----------------------------------------------------------
        var fields = new List<QueryFieldModel>();

        foreach (var member in symbol.GetMembers())
        {
            ct.ThrowIfCancellationRequested();

            if (member is not IFieldSymbol field) continue;
            // Silently skip anything that isn't an engine QueryKey<...> — the class
            // is allowed to hold other static fields.
            if (field.Type is not INamedTypeSymbol fieldKey) continue;
            if (!SymbolEqualityComparer.Default.Equals(fieldKey.OriginalDefinition, queryKey)) continue;

            var location = LocationInfo.From(field.Locations.FirstOrDefault()) ?? LocationInfo.Empty;

            // ---- read the attributes (matched by exact metadata name) ---------
            bool ignore = false;
            bool isRelation = false;
            var tagTypes = new List<string>();

            foreach (var attribute in field.GetAttributes())
            {
                if (attribute.AttributeClass is not { } attributeClass) continue;

                var definition = attributeClass.OriginalDefinition;
                string metadataName = MetadataName(definition);

                if (metadataName == WellKnown.QueryGenIgnoreAttribute)
                {
                    ignore = true;
                }
                else if (metadataName == WellKnown.QueryRelationAttribute)
                {
                    isRelation = true;
                }
                else if (metadataName == WellKnown.QueryTagAttribute)
                {
                    if (attributeClass.TypeArguments.Length == 1)
                    {
                        tagTypes.Add(attributeClass.TypeArguments[0]
                            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                    }
                    else
                    {
                        Add(diagnostics, Descriptors.BadQueryTagUsage, location, field.Name);
                    }
                }
                else if (definition.Name is "QueryGenIgnoreAttribute"
                                      or "QueryRelationAttribute"
                                      or "QueryTagAttribute")
                {
                    // Same attribute, wrong location → the WellKnown constant drifted.
                    Add(diagnostics, Descriptors.AttributeMetadataNameMismatch, location,
                        definition.Name, metadataName);
                }
            }

            if (ignore) continue;

            // ---- validate the field shape --------------------------------------
            if (!field.IsStatic)
            {
                Add(diagnostics, Descriptors.FieldMustBeStatic, location, field.Name);
                continue;
            }

            if (field.IsReadOnly)
            {
                Add(diagnostics, Descriptors.FieldMustNotBeReadOnly, location, field.Name);
                continue;
            }

            if (fieldKey.TypeArguments.Length != 1
                || fieldKey.TypeArguments[0] is not INamedTypeSymbol queryType
                || MetadataName(queryType.OriginalDefinition) != ArchetypeQueryName(queryType.Arity))
            {
                Add(diagnostics, Descriptors.QueryMustBeArchetypeQuery, location,
                    field.Name, field.Type.ToDisplayString());
                continue;
            }

            var components = queryType.TypeArguments
                .Select(t => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .ToList();

            if (isRelation && (components.Count != 1 || tagTypes.Count > 0))
            {
                Add(diagnostics, Descriptors.RelationNeedsSingleTypeArgument, location, field.Name);
                continue;
            }

            if (components.Count == 0 && tagTypes.Count == 0)
            {
                Add(diagnostics, Descriptors.QueryNeedsComponentsOrTags, location, field.Name);
                continue;
            }

            // ---- engine API arity checks (catches "engine doesn't define
            //      Query<T1,T2> yet" before it becomes a cryptic CS error) ------
            if (storeType is not null)
            {
                if (!isRelation && !HasMethod(storeType, "Query", components.Count))
                {
                    Add(diagnostics, Descriptors.EngineApiMissing, location,
                        $"Store.Query (with {components.Count} type argument(s))");
                    continue;
                }

                if (isRelation && !HasMethod(storeType, "QueryRelation", 1))
                {
                    Add(diagnostics, Descriptors.EngineApiMissing, location, "Store.QueryRelation<T>");
                    continue;
                }
            }

            if (tagTypes.Count > 0 && (tagsType is null || !HasMethod(tagsType, "Get", 1)))
            {
                Add(diagnostics, Descriptors.EngineApiMissing, location, "Tags.Get<T>");
                continue;
            }

            if (tagTypes.Count > 0 && !HasMember(queryType, "AllTags"))
            {
                Add(diagnostics, Descriptors.EngineApiMissing, location, "ArchetypeQuery.AllTags");
                continue;
            }

            fields.Add(new QueryFieldModel(
                field.Name,
                isRelation,
                new EquatableArray<string>(components.ToImmutableArray()),
                new EquatableArray<string>(tagTypes.ToImmutableArray())));
        }

        // ---- namespace / nesting -----------------------------------------------
        string? ns = symbol.ContainingNamespace is { IsGlobalNamespace: false } containingNamespace
            ? containingNamespace.ToDisplayString()
            : null;

        var nesting = new List<NestingType>();
        for (var containing = symbol.ContainingType; containing is not null; containing = containing.ContainingType)
        {
            nesting.Add(new NestingType(containing.Name, containing.IsStatic));
        }
        nesting.Reverse();

        return new QueryClassModel(
            ns,
            new EquatableArray<NestingType>(nesting.ToImmutableArray()),
            symbol.Name,
            symbol.IsStatic,
            systemCtx?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            tagsType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            callRegisterManual,
            canEmit,
            new EquatableArray<QueryFieldModel>(fields.ToImmutableArray()),
            new EquatableArray<DiagnosticInfo>(diagnostics.ToImmutableArray()));
    }

    public void Execute(SourceProductionContext context)
    {
        foreach (var diagnostic in Diagnostics.Values)
        {
            diagnostic.Report(context);
        }

        if (!EmitRegister || SystemCtxFqn is null) return;

        context.AddSource(BuildHintName(), SourceText.From(BuildSource(), Encoding.UTF8));
    }

    private string BuildHintName()
    {
        var sb = new StringBuilder();
        if (Namespace is not null) sb.Append(Namespace).Append('.');
        foreach (var containing in Nesting.Values) sb.Append(containing.Name).Append('.');
        sb.Append(ClassName).Append(".QueryRegistration.g.cs");
        return sb.ToString();
    }

    private string BuildSource()
    {
        var sb = new StringBuilder(2048);

        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//   Generated by Monod.SourceGenerators.Queries.QueryRegistrationGenerator.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("#pragma warning disable CS0612, CS0618 // obsolete members");
        sb.AppendLine();

        if (Namespace is not null)
        {
            sb.Append("namespace ").Append(Namespace).AppendLine(";");
            sb.AppendLine();
        }

        int indent = 0;

        foreach (var containing in Nesting.Values)
        {
            AppendIndent(sb, indent);
            if (containing.IsStatic) sb.Append("static ");
            sb.AppendLine("partial class " + containing.Name);
            AppendIndent(sb, indent);
            sb.AppendLine("{");
            indent++;
        }

        AppendIndent(sb, indent);
        if (IsStatic) sb.Append("static ");
        sb.AppendLine("partial class " + ClassName);
        AppendIndent(sb, indent);
        sb.AppendLine("{");
        indent++;

        AppendIndent(sb, indent);
        sb.Append("public static void Register(").Append(SystemCtxFqn).AppendLine(" ctx)");
        AppendIndent(sb, indent);
        sb.AppendLine("{");
        indent++;

        if (Fields.Values.Length > 0)
        {
            AppendIndent(sb, indent);
            sb.AppendLine("var store = ctx.Store;");
            sb.AppendLine();
        }

        foreach (var field in Fields.Values)
        {
            AppendIndent(sb, indent);
            sb.Append(field.FieldName).Append(" = ctx.RegisterQuery(");

            if (field.IsRelation)
            {
                sb.Append("store.QueryRelation<")
                  .Append(string.Join(", ", field.Components.Values))
                  .Append(">()");
            }
            else if (field.Components.Values.Length == 0)
            {
                sb.Append("store.Query()");
                AppendTags(sb, field);
            }
            else
            {
                sb.Append("store.Query<")
                  .Append(string.Join(", ", field.Components.Values))
                  .Append(">()");
                AppendTags(sb, field);
            }

            sb.AppendLine(");");
        }

        if (CallRegisterManual)
        {
            sb.AppendLine();
            AppendIndent(sb, indent);
            sb.AppendLine("RegisterManual(ctx);");
        }

        indent--;
        AppendIndent(sb, indent);
        sb.AppendLine("}"); // Register

        indent--;
        AppendIndent(sb, indent);
        sb.AppendLine("}"); // class

        for (int i = Nesting.Values.Length - 1; i >= 0; i--)
        {
            indent--;
            AppendIndent(sb, indent);
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private void AppendTags(StringBuilder sb, QueryFieldModel field)
    {
        if (field.Tags.Values.Length == 0) return;

        sb.Append(".AllTags(");
        for (int i = 0; i < field.Tags.Values.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(TagsFqn).Append(".Get<").Append(field.Tags.Values[i]).Append(">()");
        }
        sb.Append(")");
    }

    private static void AppendIndent(StringBuilder sb, int indent) => sb.Append(' ', indent * 4);

    // ---- symbol helpers ------------------------------------------------------

    private static void Add(
        List<DiagnosticInfo> diagnostics,
        DiagnosticDescriptor descriptor,
        LocationInfo location,
        params string[] args) =>
        diagnostics.Add(new DiagnosticInfo(
            descriptor.Id,
            location,
            new EquatableArray<string>(args.ToImmutableArray())));

    private static string ArchetypeQueryName(int arity) =>
        WellKnown.ArchetypeQueryType + (arity > 0 ? "`" + arity : string.Empty);

    private static string MetadataName(INamedTypeSymbol type)
    {
        var sb = new StringBuilder();
        if (!type.ContainingNamespace.IsGlobalNamespace)
        {
            sb.Append(type.ContainingNamespace.ToDisplayString()).Append('.');
        }
        for (var containing = type.ContainingType; containing is not null; containing = containing.ContainingType)
        {
            sb.Append(containing.Name);
            if (containing.Arity > 0) sb.Append('`').Append(containing.Arity);
            sb.Append('.');
        }
        sb.Append(type.Name);
        if (type.Arity > 0) sb.Append('`').Append(type.Arity);
        return sb.ToString();
    }

    private static bool IsDeclaredPartial(INamedTypeSymbol symbol, CancellationToken ct)
    {
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            ct.ThrowIfCancellationRequested();
            if (reference.GetSyntax(ct) is ClassDeclarationSyntax declaration
                && declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return true;
            }
        }
        return false;
    }

    private static bool ParameterAccepts(ITypeSymbol parameterType, INamedTypeSymbol systemCtx)
    {
        if (SymbolEqualityComparer.Default.Equals(parameterType, systemCtx)) return true;

        for (var baseType = systemCtx.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(parameterType, baseType)) return true;
        }

        return systemCtx.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(parameterType, i));
    }

    private static bool HasMethod(INamedTypeSymbol? type, string name, int typeParameterCount)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.GetMembers(name).OfType<IMethodSymbol>()
                .Any(m => m.TypeParameters.Length == typeParameterCount))
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasMember(INamedTypeSymbol? type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.GetMembers(name).Length > 0) return true;
        }
        return false;
    }
}

internal sealed record NestingType(string Name, bool IsStatic);

internal sealed record QueryFieldModel(
    string FieldName,
    bool IsRelation,
    EquatableArray<string> Components,   // fully-qualified component types
    EquatableArray<string> Tags);        // fully-qualified tag types

internal sealed record DiagnosticInfo(
    string DescriptorId,
    LocationInfo Location,
    EquatableArray<string> MessageArgs)
{
    public void Report(SourceProductionContext context)
    {
        if (Descriptors.Find(DescriptorId) is not { } descriptor) return;

        context.ReportDiagnostic(Diagnostic.Create(
            descriptor,
            Location.ToLocation(),
            MessageArgs.Values.ToArray()));
    }
}

// Location isn't equatable, so we snapshot it into data. Without this the
// model records wouldn't be value-comparable and caching would break again.
internal sealed record LocationInfo(
    string? FilePath,
    int TextStart,
    int TextLength,
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter)
{
    public static readonly LocationInfo Empty = new("", 0, 0, 0, 0, 0, 0);

    public static LocationInfo? From(Location? location)
    {
        if (location?.SourceTree is not { } tree) return null;

        var span = location.SourceSpan;
        var lineSpan = location.GetLineSpan();

        return new LocationInfo(
            tree.FilePath,
            span.Start,
            span.Length,
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line,
            lineSpan.EndLinePosition.Character);
    }

    public Location ToLocation() => Location.Create(
        FilePath ?? string.Empty,
        new TextSpan(TextStart, TextLength),
        new LinePositionSpan(
            new LinePosition(StartLine, StartCharacter),
            new LinePosition(EndLine, EndCharacter)));
}

// ImmutableArray<T> has no value equality, which would silently break the
// incremental cache — same reason we can't pass ISymbol through the pipeline.
internal sealed class EquatableArray<T> : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public EquatableArray(ImmutableArray<T> values) => Values = values;

    public ImmutableArray<T> Values { get; }

    public bool Equals(EquatableArray<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        var a = Values;
        var b = other.Values;
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(a[i], b[i])) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var value in Values) hash = (hash * 31) + value.GetHashCode();
        return hash;
    }

    public static bool operator ==(EquatableArray<T>? left, EquatableArray<T>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(EquatableArray<T>? left, EquatableArray<T>? right) => !(left == right);
}