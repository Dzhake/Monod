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
        // [MQuery<...>] is a family of generic attributes — one per arity — and
        // ForAttributeWithMetadataName matches the exact metadata name including
        // the `N arity suffix. So one pipeline is registered per arity; every
        // pipeline builds the SAME whole-class model (the transform reads all
        // [MQuery] attributes off the class symbol), and Distinct() below
        // collapses the duplicates. Editing an unrelated file triggers zero work.
        var collected = new List<IncrementalValueProvider<ImmutableArray<QueryClassModel>>>();

        for (int arity = 1; arity <= WellKnown.MaxMQueryArity; arity++)
        {
            collected.Add(
                context.SyntaxProvider.ForAttributeWithMetadataName(
                    WellKnown.MQueryAttributeMetadataName(arity),
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) =>
                        QueryClassModel.Create((INamedTypeSymbol)ctx.TargetSymbol, ctx.SemanticModel.Compilation, ct))
                .Collect());
        }

        var models = collected[0];
        for (int i = 1; i < collected.Count; i++)
        {
            var next = collected[i];
            models = models.Combine(next).Select(static (pair, _) => pair.Left.AddRange(pair.Right));
        }

        // No CompilationProvider.Combine — it re-runs the output stage on every
        // keystroke project-wide. All compilation access happens in the transform.
        context.RegisterSourceOutput(models, static (spc, allModels) =>
        {
            // Distinct guards against the same class being reached through several
            // [MQuery] arities (and against attributes on two partial parts).
            foreach (var model in allModels.Distinct())
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
    // ---- the [MQuery<...>] attribute family ----------------------------------
    // Generic attributes exist once per arity, so the generator needs one
    // pipeline per arity (see QueryRegistrationGenerator.Initialize). Keep
    // MaxMQueryArity in sync with the highest MQueryAttribute<...> arity
    // declared in the engine.
    public const string MQueryAttributeBase = "Monod.ECS.Systems.MQueryAttribute";
    public const int MaxMQueryArity = 10;

    public static string MQueryAttributeMetadataName(int arity) => $"{MQueryAttributeBase}`{arity}";

    public static bool IsMQueryAttributeMetadataName(string metadataName)
    {
        if (!metadataName.StartsWith(MQueryAttributeBase + "`", StringComparison.Ordinal)) return false;

        string arityText = metadataName[(MQueryAttributeBase.Length + 1)..];
        return int.TryParse(arityText, out int arity) && arity is >= 1 and <= MaxMQueryArity;
    }

    // Engine marker interfaces — used to auto-classify [MQuery] type arguments
    // into components, relations and tags. Tags never contribute to the
    public const string ComponentInterfaceType = "Friflo.Engine.ECS.IComponent";
    public const string RelationInterfaceType = "Friflo.Engine.ECS.IRelation`1";
    public const string TagInterfaceType = "Friflo.Engine.ECS.ITag";

    // Engine types — adjust to your real namespaces.
    public const string SystemCtxType = "Monod.ECS.Systems.SystemCtx";
    public const string QueryKeyType = "Monod.ECS.Systems.QueryKey`1";
    public const string TagsType = "Friflo.Engine.ECS.Tags";
    public const string ArchetypeQueryType = "Friflo.Engine.ECS.ArchetypeQuery";

    public static string ArchetypeQueryMetadataName(int arity) =>
        ArchetypeQueryType + (arity > 0 ? "`" + arity : string.Empty);
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

    public static readonly DiagnosticDescriptor RelationNeedsSingleTypeArgument = Make("MGQ015",
        "Relation queries take exactly one relation type",
        "The [MQuery] query '{0}' contains a relation type; relation queries must list exactly one relation type and no component types (tags are allowed).");

    public static readonly DiagnosticDescriptor RegisterIsGenerated = Make("MGQ017",
        "Register is generated",
        "This Register method is generated by QueryRegistrationGenerator. Delete it and move custom code into 'static void RegisterManual(SystemCtx ctx)', which the generator calls automatically.");

    public static readonly DiagnosticDescriptor BadRegisterManualSignature = Make("MGQ018",
        "Invalid RegisterManual signature",
        "RegisterManual must be 'static void RegisterManual({1} ctx)'; found '{0}'.");

    public static readonly DiagnosticDescriptor AmbiguousRegisterManual = Make("MGQ019",
        "Ambiguous RegisterManual",
        "Multiple valid RegisterManual overloads were found; keep exactly one.");

    public static readonly DiagnosticDescriptor EngineApiMissing = Make("MGQ021",
        "Engine API missing",
        "Engine member '{0}' could not be found; the engine and the generator are out of sync.");

    public static readonly DiagnosticDescriptor MQueryTypeNotRecognized = Make("MGQ022",
        "Type is not a component, relation or tag",
        "The type '{0}' in [MQuery] does not implement the engine's IComponent, IRelation or ITag marker interfaces, so the generator cannot classify it.");

    public static readonly DiagnosticDescriptor MQueryFieldNameMustBeString = Make("MGQ023",
        "Query field name must be a string",
        "The first constructor argument of [MQuery] must be a string containing the query field name; found '{0}'.");

    public static readonly DiagnosticDescriptor MQueryFieldNameInvalid = Make("MGQ024",
        "Invalid query field name",
        "The query field name '{0}' is not a valid C# identifier.");

    public static readonly DiagnosticDescriptor MQueryFieldNameCollision = Make("MGQ025",
        "Query field name collision",
        "The query field name '{0}' collides with another member of '{1}' or with another [MQuery] query; give one of them an explicit name.");

    public static DiagnosticDescriptor? Find(string id) => id switch
    {
        "MGQ001" => EngineTypeMissing,
        "MGQ002" => AttributeMetadataNameMismatch,
        "MGQ010" => ClassMustBePartial,
        "MGQ011" => ClassMustNotBeGeneric,
        "MGQ015" => RelationNeedsSingleTypeArgument,
        "MGQ017" => RegisterIsGenerated,
        "MGQ018" => BadRegisterManualSignature,
        "MGQ019" => AmbiguousRegisterManual,
        "MGQ021" => EngineApiMissing,
        "MGQ022" => MQueryTypeNotRecognized,
        "MGQ023" => MQueryFieldNameMustBeString,
        "MGQ024" => MQueryFieldNameInvalid,
        "MGQ025" => MQueryFieldNameCollision,
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
    string? QueryKeyFqn,                     // 'global::Monod.ECS.Systems.QueryKey'
    string? ArchetypeQueryFqn,               // 'global::Friflo.Engine.ECS.ArchetypeQuery'
    string? TagsFqn,                         // 'global::Friflo.Engine.ECS.Tags'
    bool CallRegisterManual,
    bool EmitRegister,                       // false → report diagnostics only
    EquatableArray<QueryFieldModel> Fields,  // generated fields, in attribute order
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
        var archetypeQuery = compilation.GetTypeByMetadataName(WellKnown.ArchetypeQueryType);
        var tagsType = compilation.GetTypeByMetadataName(WellKnown.TagsType);
        var componentInterface = compilation.GetTypeByMetadataName(WellKnown.ComponentInterfaceType);
        var relationInterface = compilation.GetTypeByMetadataName(WellKnown.RelationInterfaceType);
        var tagInterface = compilation.GetTypeByMetadataName(WellKnown.TagInterfaceType);

        if (systemCtx is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.SystemCtxType);
        if (queryKey is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.QueryKeyType);
        if (archetypeQuery is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.ArchetypeQueryType);
        if (tagsType is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.TagsType);
        if (componentInterface is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.ComponentInterfaceType);
        if (relationInterface is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.RelationInterfaceType);
        if (tagInterface is null) Add(diagnostics, Descriptors.EngineTypeMissing, classLocation, WellKnown.TagInterfaceType);

        bool canEmit = systemCtx is not null
            && queryKey is not null
            && archetypeQuery is not null
            && componentInterface is not null
            && relationInterface is not null
            && tagInterface is not null;

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

        // ---- [MQuery<...>] attributes → generated fields ----------------------
        var fields = new List<QueryFieldModel>();
        var usedFieldNames = new HashSet<string>(StringComparer.Ordinal);

        if (componentInterface is not null && relationInterface is not null && tagInterface is not null)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                ct.ThrowIfCancellationRequested();

                if (attribute.AttributeClass is not { } attributeClass) continue;

                var definition = attributeClass.OriginalDefinition;
                string metadataName = MetadataName(definition);

                if (!WellKnown.IsMQueryAttributeMetadataName(metadataName))
                {
                    // An attribute *named* MQueryAttribute the generator does not know —
                    // wrong namespace, or an arity above MaxMQueryArity. A non-generic
                    // legacy MQueryAttribute is left alone.
                    if (definition.Name == "MQueryAttribute"
                        && metadataName != WellKnown.MQueryAttributeBase)
                    {
                        Add(diagnostics, Descriptors.AttributeMetadataNameMismatch,
                            AttributeLocation(attribute, classLocation, ct), definition.Name, metadataName);
                    }
                    continue;
                }

                var location = AttributeLocation(attribute, classLocation, ct);

                // ---- classify the type arguments (attribute order preserved) ----
                var components = new List<string>();
                var relations = new List<string>();
                var tags = new List<string>();
                var defaultName = new StringBuilder();
                bool broken = false;

                foreach (var typeArgument in attributeClass.TypeArguments)
                {
                    if (typeArgument.TypeKind == TypeKind.Error)
                    {
                        // The attribute references an unresolvable type — the
                        // compilation is broken anyway; stay quiet.
                        broken = true;
                        break;
                    }

                    if (Classify(typeArgument, componentInterface, relationInterface, tagInterface) is not { } entry)
                    {
                        Add(diagnostics, Descriptors.MQueryTypeNotRecognized, location,
                            typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                        broken = true;
                        break;
                    }

                    string fqn = entry.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                    switch (entry.Kind)
                    {
                    case QueryTypeKind.Component:
                        components.Add(fqn);
                        break;
                    case QueryTypeKind.Relation:
                        relations.Add(fqn);
                        break;
                    case QueryTypeKind.Tag:
                        tags.Add(fqn);
                        break;
                    }

                    if (defaultName.Length > 0) defaultName.Append('_');
                    defaultName.Append(entry.Type.Name);
                }

                if (broken) continue;

                // ---- field name: first constructor argument, else the type names ----
                string fieldName;
                var firstArgument = attribute.ConstructorArguments.Length > 0
                    ? attribute.ConstructorArguments[0]
                    : default;

                if (firstArgument.Value is string provided)
                {
                    fieldName = provided;
                }
                else if (firstArgument.Value is not null)
                {
                    Add(diagnostics, Descriptors.MQueryFieldNameMustBeString, location,
                        firstArgument.Value!.ToString() ?? "null");
                    continue;
                }
                else
                {
                    // No constructor argument, or an explicit null → default name.
                    fieldName = defaultName.ToString();
                }

                if (!SyntaxFacts.IsValidIdentifier(fieldName))
                {
                    Add(diagnostics, Descriptors.MQueryFieldNameInvalid, location, fieldName);
                    continue;
                }

                // ---- relation queries: exactly one relation, no components ----
                if (relations.Count > 0 && (relations.Count > 1 || components.Count > 0))
                {
                    Add(diagnostics, Descriptors.RelationNeedsSingleTypeArgument, location, fieldName);
                    continue;
                }

                // ---- name collisions: other [MQuery]s, hand-written members and the
                //      generated Register method itself ------------------------------
                if (!usedFieldNames.Add(fieldName)
                    || symbol.GetMembers(fieldName).Length > 0
                    || fieldName == "Register")
                {
                    Add(diagnostics, Descriptors.MQueryFieldNameCollision, location, fieldName, symbol.Name);
                    continue;
                }

                // ---- engine API arity checks (catches "engine doesn't define
                //      ArchetypeQuery<T1..T7> / Query<T1, T2> yet" before it becomes
                //      a cryptic CS error) -----------------------------------------
                bool isRelation = relations.Count == 1;
                var queryTypeArgs = isRelation ? relations : components;

                if (queryTypeArgs.Count > 0
                    && compilation.GetTypeByMetadataName(WellKnown.ArchetypeQueryMetadataName(queryTypeArgs.Count)) is null)
                {
                    Add(diagnostics, Descriptors.EngineApiMissing, location,
                        $"ArchetypeQuery (with {queryTypeArgs.Count} type argument(s))");
                    continue;
                }

                if (isRelation)
                {
                    if (storeType is not null && !HasMethod(storeType, "QueryRelation", 1))
                    {
                        Add(diagnostics, Descriptors.EngineApiMissing, location, "Store.QueryRelation<T>");
                        continue;
                    }
                }
                else if (components.Count > 0)
                {
                    if (storeType is not null && !HasMethod(storeType, "Query", components.Count))
                    {
                        Add(diagnostics, Descriptors.EngineApiMissing, location,
                            $"Store.Query (with {components.Count} type argument(s))");
                        continue;
                    }
                }
                else if (storeType is not null && !HasMethod(storeType, "Query", 0))
                {
                    Add(diagnostics, Descriptors.EngineApiMissing, location, "Store.Query()");
                    continue;
                }

                if (tags.Count > 0 && (tagsType is null || !HasMethod(tagsType, "Get", tags.Count)))
                {
                    Add(diagnostics, Descriptors.EngineApiMissing, location,
                        $"Tags.Get<T> (with {tags.Count} type argument(s))");
                    continue;
                }

                fields.Add(new QueryFieldModel(
                    fieldName,
                    isRelation,
                    new EquatableArray<string>(queryTypeArgs.ToImmutableArray()),
                    new EquatableArray<string>(tags.ToImmutableArray())));
            }
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
            GlobalFqn(systemCtx),
            GlobalFqn(queryKey),
            GlobalFqn(archetypeQuery),
            GlobalFqn(tagsType),
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

        if (!EmitRegister
            || SystemCtxFqn is null
            || QueryKeyFqn is null
            || ArchetypeQueryFqn is null) return;

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

        // ---- the generated query fields ----
        foreach (var field in Fields.Values)
        {
            AppendIndent(sb, indent);
            sb.Append("public static ").Append(QueryKeyFqn).Append('<').Append(ArchetypeQueryFqn);
            if (field.QueryTypeArgs.Values.Length > 0)
            {
                sb.Append('<').Append(string.Join(", ", field.QueryTypeArgs.Values)).Append('>');
            }
            sb.Append("> ").Append(field.FieldName).AppendLine(";");
        }

        if (Fields.Values.Length > 0) sb.AppendLine();

        // ---- Register ----
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
                  .Append(string.Join(", ", field.QueryTypeArgs.Values))
                  .Append(">()");
            }
            else if (field.QueryTypeArgs.Values.Length == 0)
            {
                sb.Append("store.Query()");
            }
            else
            {
                sb.Append("store.Query<")
                  .Append(string.Join(", ", field.QueryTypeArgs.Values))
                  .Append(">()");
            }

            AppendTags(sb, field);
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

        sb.Append(".AllTags(")
          .Append(TagsFqn)
          .Append(".Get<")
          .Append(string.Join(", ", field.Tags.Values))
          .Append(">())");
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

    private static LocationInfo AttributeLocation(AttributeData attribute, LocationInfo fallback, CancellationToken ct)
    {
        var syntax = attribute.ApplicationSyntaxReference?.GetSyntax(ct);
        return syntax is null ? fallback : LocationInfo.From(syntax.GetLocation()) ?? fallback;
    }

    /// <summary>
    /// Classifies a [MQuery] type argument. Order matters: relations are
    /// components too (IRelation&lt;T&gt; : IComponent), tags are not.
    /// </summary>
    private static (QueryTypeKind Kind, INamedTypeSymbol Type)? Classify(
        ITypeSymbol type,
        INamedTypeSymbol componentInterface,
        INamedTypeSymbol relationInterface,
        INamedTypeSymbol tagInterface)
    {
        if (type is not INamedTypeSymbol named) return null;

        if (ImplementsMarker(named, tagInterface)) return (QueryTypeKind.Tag, named);
        if (ImplementsMarker(named, relationInterface)) return (QueryTypeKind.Relation, named);
        if (ImplementsMarker(named, componentInterface)) return (QueryTypeKind.Component, named);

        return null;
    }

    private static bool ImplementsMarker(INamedTypeSymbol type, INamedTypeSymbol marker)
    {
        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, marker)) return true;

        foreach (var implemented in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, marker)) return true;
        }

        return false;
    }

    /// <summary>
    /// Unbound generic definitions render as "QueryKey&lt;T&gt;"; the generator
    /// appends its own type arguments, so only the open name is wanted.
    /// </summary>
    private static string? GlobalFqn(INamedTypeSymbol? type)
    {
        if (type is null) return null;

        string metadataName = MetadataName(type);
        int arityIndex = metadataName.LastIndexOf('`');
        return "global::" + (arityIndex > 0 ? metadataName[..arityIndex] : metadataName);
    }

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

internal enum QueryTypeKind
{
    Component,
    Relation,
    Tag
}

internal sealed record NestingType(string Name, bool IsStatic);

internal sealed record QueryFieldModel(
    string FieldName,
    bool IsRelation,
    EquatableArray<string> QueryTypeArgs,   // fully-qualified component types, or the single relation type; tags excluded
    EquatableArray<string> Tags);           // fully-qualified tag types

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