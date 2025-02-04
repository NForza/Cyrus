using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NForza.Cyrus.Generators.TypedIds;

[Generator]
public class GuidIdTypeConverterGenerator : TypedIdGeneratorBase, IIncrementalGenerator
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
                var sourceText = GenerateGuidIdTypeConverter(recordSymbol);
                spc.AddSource($"{recordSymbol}TypeConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };
        });
    }


    private string GenerateGuidIdTypeConverter(INamedTypeSymbol item)
    {
        var model = new
        {
            TypedIdName = item.Name,
            NamespaceName = item.ContainingNamespace.ToDisplayString()
        };

        string source = LiquidEngine.Render(model, "GuidIdTypeConverter");

        return source;
    }
}