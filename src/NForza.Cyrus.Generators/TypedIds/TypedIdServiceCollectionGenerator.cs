using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

public class TypedIdInitializerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var typedIds = cyrusGenerationContext.TypedIds;
        var config = cyrusGenerationContext.GenerationConfig;
        var compilation = cyrusGenerationContext.Compilation;

        if (!config.GenerationTarget.Contains(GenerationTarget.WebApi))
            return;

        var referencedTypedIds = compilation.GetAllTypesFromCyrusAssemblies()
            .Where(nts => nts.IsTypedId());

        var allTypedIds = typedIds.Concat(referencedTypedIds).ToArray();

        var sourceText = GenerateServiceCollectionExtensionMethod(allTypedIds, cyrusGenerationContext.LiquidEngine);
        spc.AddSource("TypedIdInitializer.g.cs", sourceText);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    private string GenerateServiceCollectionExtensionMethod(IEnumerable<INamedTypeSymbol> typedIds, LiquidEngine liquidEngine)
    {
        var converters = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<JsonConverter, {t.Name}JsonConverter>();"));

        var imports = typedIds
            .Select(t => t.ContainingNamespace.GetNameOrEmpty())
            .Where(s => !string.IsNullOrEmpty(s))
            .Concat(["System", "System.Collections.Generic"])
            .Distinct();

        var types = typedIds.Select(t => new { Name = t.ToFullName(), UnderlyingType = t.GetUnderlyingTypeOfTypedId() }).ToList();
        var registrations = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<{t.ToDisplayString()}>();"));

        var model = new Dictionary<string, object>
        {
            ["Types"] = typedIds.Select(t => new Dictionary<string, object>
            {
                ["Name"] = t.ToFullName(),
                ["UnderlyingType"] = t.GetUnderlyingTypeOfTypedId()
            }).ToList(),

            ["Imports"] = imports.ToList()
        };

        var source = liquidEngine.Render(model, "ServiceCollectionJsonConverterExtensions");
        return source;
    }
}
