using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.TypedIds.Generator;

namespace NForza.Cyrus.Generators.TypedIds;

[Generator]
public class IntIdGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(false);
        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => IsRecordWithIntIdAttribute(syntaxNode),
                        transform: (context, _) => GetNamedTypeSymbolFromContext(context));

        var recordStructsWithAttribute = incrementalValuesProvider
            .Where(x => x is not null)
            .Select((x, _) => x!)
            .Collect();

        context.RegisterSourceOutput(recordStructsWithAttribute, (spc, recordSymbols) =>
        {
            foreach (var recordSymbol in recordSymbols)
            {
                var sourceText = GenerateIntId(recordSymbol);
                spc.AddSource($"{recordSymbol.Name}.g.cs", SourceText.From(sourceText, Encoding.UTF8));
            };

            if (recordSymbols.Any())
            {
                var intModels = GetPartialModelClass(recordSymbols.First().ContainingAssembly.Name, "Integers", "string", recordSymbols.Select(im => $"\"{im.Name}\""));
                spc.AddSource($"model-ints.g.cs", SourceText.From(intModels, Encoding.UTF8));
            }
        });
    }

    private string GenerateIntId(INamedTypeSymbol item)
    {
        var replacements = new Dictionary<string, string>
        {
            ["ItemName"] = item.Name,
            ["Namespace"] = item.ContainingNamespace.ToDisplayString(),
            ["CastOperators"] = GenerateCastOperatorsToUnderlyingType(item),
            ["IsValid"] = GetIsValidExpression(item)
        };

        var source = TemplateEngine.ReplaceInResourceTemplate("IntId.cs", replacements);

        return source;
    }

    private string GetIsValidExpression(INamedTypeSymbol item)
    {
        var attribute = item.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "IntIdAttribute");
        if (attribute == null)
        {
            return string.Empty;
        }
        var args = attribute.ConstructorArguments.Select(a => int.Parse(a.Value?.ToString())).ToList();
        var min = args.Count > 0 ? args[0] : -1;
        var max = args.Count > 1 ? args[1] : -1;
        if (min < 0 && max < 0)
        {
            return string.Empty;
        }
        var minExpression = min < 0 ? string.Empty : $"Value >= {min}";
        var maxExpression = max < 0 ? string.Empty : $"Value <= {max}";
        var AndOperator = minExpression.Length > 0 && maxExpression.Length > 0 ? " && " : "";

        return $"public bool IsValid() => {minExpression}{AndOperator}{maxExpression};";
    }

    private string GenerateCastOperatorsToUnderlyingType(INamedTypeSymbol item) =>
        @$"public static implicit operator {GetUnderlyingTypeOfTypedId(item)}({item.ToDisplayString()} typedId) => typedId.Value;
    public static explicit operator {item.ToDisplayString()}({GetUnderlyingTypeOfTypedId(item)} value) => new(value);";
}