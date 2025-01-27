using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.TypedIds.Generator;

namespace NForza.Cyrus.Generators.TypedIds;

[Generator]
public class StringIdGenerator : TypedIdGeneratorBase, IIncrementalGenerator
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
                var sourceText = GenerateCodeForRecordStruct(recordSymbol);
                spc.AddSource($"{recordSymbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };
            if (recordSymbols.Any())
            {
                var stringModels = GetPartialModelClass(recordSymbols.First().ContainingAssembly.Name, "Strings", "string", recordSymbols.Select(s => $"\"{s.Name}\""));
                spc.AddSource($"model-strings.g.cs", SourceText.From(stringModels, Encoding.UTF8));
            }
        });
    }

    private string GenerateCodeForRecordStruct(INamedTypeSymbol item)
    {
        (int? min, int? max) = GetMinAndMaxFromType(item);
        var model = new 
        {
            item.Name,
            Namespace = item.ContainingNamespace.ToDisplayString(),
            UnderlyingType = GetUnderlyingTypeOfTypedId(item),
            Minimum = min,
            HasMinimum = min.HasValue,
            Maximum = max,
            HasMaximum = max.HasValue,
            HasMaximumAndMinumum = min.HasValue && max.HasValue
        };

        var resolvedSource = ScribanEngine.Render("StringId", model);

        return resolvedSource;
    }

    private (int? min, int? max) GetMinAndMaxFromType(INamedTypeSymbol item)
    {
        var attribute = item.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "StringIdAttribute");
        if (attribute == null)
        {
            return (null, null);
        }
        var args = attribute.ConstructorArguments.Select(a => int.Parse(a.Value?.ToString())).ToList();
        int? min = args.Count > 0 ? args[0] : null;
        int? max = args.Count > 1 ? args[1] : null;
        return (min, max);
    }
}