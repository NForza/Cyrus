using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.TypedIds;

namespace NForza.Cyrus.TypedIds.Generator;

[Generator]
public class TypedIdServiceCollectionGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var typedIdsCollectionProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => IsRecordWithStringIdAttribute(syntaxNode) || IsRecordWithGuidIdAttribute(syntaxNode) || IsRecordWithIntIdAttribute(syntaxNode),
                        transform: (context, _) => GetNamedTypeSymbolFromContext(context));

        var configProvider = ConfigProvider(context);
        var compilationProvider = context.CompilationProvider;

        var typedIdsProvider = typedIdsCollectionProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect()
            .Combine(configProvider)
            .Combine(compilationProvider);

        context.RegisterSourceOutput(typedIdsProvider, (spc, typedIdsWithConfig) =>
        {
            var ((typedIds, config), compilation) = typedIdsWithConfig;

            if (!config.GenerationTarget.Contains(GenerationTarget.WebApi))
                return;

            var referencedTypedIds = compilation.GetAllTypesFromCompilationAndReferencedAssemblies(config.Contracts)
                .Where(nts => nts.IsTypedId());

            var allTypedIds = typedIds.Concat(referencedTypedIds).ToArray();

            var sourceText = GenerateServiceCollectionExtensionMethod(allTypedIds);
            spc.AddSource("CqrsServiceCollectionExtensions.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        });
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    private string GenerateServiceCollectionExtensionMethod(IEnumerable<INamedTypeSymbol> typedIds)
    {
        var converters = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<JsonConverter, {t.Name}JsonConverter>();"));

        var namespaces = string.Join(Environment.NewLine, typedIds.Select(t => t.ContainingNamespace.ToDisplayString()).Distinct().Select(ns => $"using {ns};"));

        var types = string.Join(",", typedIds.Select(t => $"[typeof({t.ToFullName()})] = typeof({GetUnderlyingTypeOfTypedId(t)})"));
        var registrations = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<{t.ToDisplayString()}>();"));

        var replacements = new Dictionary<string, string>()
        {
            ["AllTypes"] = types,
            ["AllTypedIdRegistrations"] = registrations,
            ["Namespaces"] = namespaces,
            ["AddJsonConverters"] = converters
        };

        var source = TemplateEngine.ReplaceInResourceTemplate("CyrusOptionsJsonConverterExtensions.cs", replacements);        
        return source;
    }
}
