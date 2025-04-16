﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.TypedIds;

public class TypedIdInitializerGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var typedIds = cyrusProvider.TypedIds;
        var config = cyrusProvider.GenerationConfig;
        var compilation = cyrusProvider.Compilation;

        if (!config.GenerationTarget.Contains(GenerationTarget.WebApi))
            return;

        var referencedTypedIds = compilation.GetAllTypesFromCyrusAssemblies()
            .Where(nts => nts.IsTypedId());

        var allTypedIds = typedIds.Concat(referencedTypedIds).ToArray();

        var sourceText = GenerateServiceCollectionExtensionMethod(allTypedIds);
        spc.AddSource("TypedIdInitializer.g.cs", SourceText.From(sourceText, Encoding.UTF8));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    private string GenerateServiceCollectionExtensionMethod(IEnumerable<INamedTypeSymbol> typedIds)
    {
        var converters = string.Join(Environment.NewLine, typedIds.Select(t => $"services.AddTransient<JsonConverter, {t.Name}JsonConverter>();"));

        var imports = typedIds
            .Select(t => t.ContainingNamespace.ToDisplayString())
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

        var source = LiquidEngine.Render(model, "ServiceCollectionJsonConverterExtensions");
        return source;
    }
}
