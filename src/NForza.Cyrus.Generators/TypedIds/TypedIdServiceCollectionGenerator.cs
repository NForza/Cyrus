using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators;
using NForza.Generators;

namespace NForza.Cyrus.TypedIds.Generator;

[Generator]
public class TypedIdServiceCollectionGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => IsRecordWithStringIdAttribute(syntaxNode) || IsRecordWithGuidIdAttribute(syntaxNode),
                        transform: (context, _) => GetNamedTypeSymbolFromContext(context));

        var typedIdsProvider = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(typedIdsProvider, (spc, typedIds) =>
        {
            if (typedIds.Length > 0)
            {
                var sourceText = GenerateServiceCollectionExtensionMethod(typedIds);
                spc.AddSource("ServiceCollectionExtensions.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            }
        });
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    private string GenerateServiceCollectionExtensionMethod(IEnumerable<INamedTypeSymbol> typedIds)
    {
        var source = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "ServiceCollectionExtensions.cs");

        var converters = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<JsonConverter, {t.Name}JsonConverter>();"));

        var namespaces = string.Join(Environment.NewLine, typedIds.Select(t => t.ContainingNamespace.ToDisplayString()).Distinct().Select(ns => $"using {ns};"));

        var types = string.Join(",", typedIds.Select(t => $"[typeof({t.ToFullName()})] = typeof({GetUnderlyingTypeOfTypedId(t)})"));
        var registrations = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<{t.ToDisplayString()}>();"));

        source = source
            .Replace("% AllTypes %", types)
            .Replace("% AllTypedIdRegistrations %", registrations)
            .Replace("% Namespaces %", namespaces)
            .Replace("% AddJsonConverters %", converters);
        return source;
    }
}
