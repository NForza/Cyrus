#if DEBUG_ANALYZER 
using System.Collections.Generic;
#endif
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class StringIdGenerator : TypedIdGeneratorBase, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
        DebugThisGenerator(false);

        var typedIds = GetAllTypedIds(context.Compilation, "StringIdAttribute");
        foreach (var item in typedIds)
        {
            GenerateStringId(context, item);
        }
    }

    private void GenerateStringId(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var replacements = new Dictionary<string, string>
        {
            ["ItemName"] = item.Name,
            ["Namespace"] = item.ContainingNamespace.ToDisplayString(),
            ["CastOperators"] = GenerateCastOperatorsToUnderlyingType(item),
            ["IsValid"] = GetIsValidExpression(item)
        };

        var resolvedSource = TemplateEngine.ReplaceInResourceTemplate("StringId.cs", replacements);

        context.AddSource($"{item.Name}.g.cs", resolvedSource);
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