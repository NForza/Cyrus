using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.TypedIds;

[Generator]
public class GuidIdGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
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
                var sourceText = GenerateGuidId(recordSymbol);
                spc.AddSource($"{recordSymbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };

            if (recordSymbols.Any())
            {
                var guidModels = GetPartialModelClass(
                    recordSymbols.First().ContainingAssembly.Name,
                    "TypedIds",
                    "Guids", 
                    "string", 
                    recordSymbols.Select(guid => $"\"{guid.Name}\""));
                spc.AddSource($"model-guids.g.cs", SourceText.From(guidModels, Encoding.UTF8));
            }

        });
    }

    private string GenerateGuidId(INamedTypeSymbol item)
    {
        var model = new
        {
            item.Name,
          Namespace = item.ContainingNamespace.ToDisplayString(),
          UnderlyingType = GetUnderlyingTypeOfTypedId(item),
          Default = "Guid.Empty"
        };

        var source = LiquidEngine.Render(model, "GuidId");

        return source;
    }
}