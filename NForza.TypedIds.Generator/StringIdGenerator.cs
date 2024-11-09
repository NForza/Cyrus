#if DEBUG_ANALYZER 
using System.Collections.Generic;
#endif
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class StringIdGenerator : TypedIdGeneratorBase, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);
        var incrementalValuesProvider = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: (syntaxNode, _) => IsRecordWithStringIdAttribute(syntaxNode),
                        transform: (context, _) => GetSemanticTargetForGeneration(context));

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
        });
    }

    private static bool IsRecordWithStringIdAttribute(SyntaxNode syntaxNode)
    {
        bool isRecordWithStringId = syntaxNode is RecordDeclarationSyntax recordDecl &&
                       recordDecl.HasAttribute("StringId");
        return isRecordWithStringId;
    }

    private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var recordStruct = (RecordDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        var symbol = model.GetDeclaredSymbol(recordStruct) as INamedTypeSymbol;
        if (symbol == null)
            return null;

        return symbol;
    }

    private string GenerateCodeForRecordStruct(INamedTypeSymbol recordSymbol)
    {
        var replacements = new Dictionary<string, string>
        {
            ["ItemName"] = recordSymbol.Name,
            ["Namespace"] = recordSymbol.ContainingNamespace.ToDisplayString(),
            ["CastOperators"] = GenerateCastOperatorsToUnderlyingType(recordSymbol),
            ["IsValid"] = GetIsValidExpression(recordSymbol)
        };

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("StringId.cs", replacements);

        return resolvedSource;
    }

    private string GetIsValidExpression(INamedTypeSymbol item)
    {
        var attribute = item.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "StringIdAttribute");
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
        var minExpression = min < 0 ? string.Empty : $" && Value.Length >= {min}";
        var maxExpression = max < 0 ? string.Empty : $" && Value.Length <= {max}";

        return $"public bool IsValid() => !string.IsNullOrEmpty(Value){minExpression}{maxExpression};";
    }

    private string GenerateCastOperatorsToUnderlyingType(INamedTypeSymbol item)
    {
        return @$"public static implicit operator {GetUnderlyingTypeOfTypedId(item)}({item.ToDisplayString()} typedId) => typedId.Value;
    public static explicit operator {item.ToDisplayString()}({GetUnderlyingTypeOfTypedId(item)} value) => new(value);";
    }
}