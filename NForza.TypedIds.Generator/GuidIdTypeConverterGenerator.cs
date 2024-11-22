using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class GuidIdTypeConverterGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => IsRecordWithGuidIdAttribute(syntaxNode),
                        transform: (context, _) => GetNamedTypeSymbolFromContext(context));

        var recordStructsWithAttribute = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(recordStructsWithAttribute, (spc, recordSymbols) =>
        {
            foreach (var recordSymbol in recordSymbols)
            {
                var sourceText = GenerateGuidIdTypeConverter(recordSymbol);
                spc.AddSource($"{recordSymbol}TypeConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };
        });
    }


    private string GenerateGuidIdTypeConverter(INamedTypeSymbol item)
    {
        var replacements = new Dictionary<string, string>
        {
            ["TypedIdName"] = item.Name,
            ["NamespaceName"] = item.ContainingNamespace.ToDisplayString()
        };

        string source = TemplateEngine.ReplaceInResourceTemplate("GuidIdTypeConverter.cs", replacements);

        return source;
    }
}