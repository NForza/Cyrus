using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NForza.Cyrus.Generators.TypedIds;

[Generator]
public class StringIdTypeConverterGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);

        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => IsRecordWithStringIdAttribute(syntaxNode),
                        transform: (context, _) => GetNamedTypeSymbolFromContext(context));

        var recordStructsWithAttribute = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(recordStructsWithAttribute, (spc, recordSymbols) =>
        {
            foreach (var recordSymbol in recordSymbols)
            {
                var sourceText = GenerateStringIdTypeConverter(recordSymbol);
                spc.AddSource($"{recordSymbol}TypeConverter.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };
        });
    }

    private string GenerateStringIdTypeConverter(INamedTypeSymbol item)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        var model = new
        {
            item.Name,
            Namespace = fullyQualifiedNamespace
        };
        var resolvedSource = LiquidEngine.Render(model, "StringIdTypeConverter");
        return resolvedSource;
    }
}