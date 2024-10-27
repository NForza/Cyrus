#if DEBUG_ANALYZER 
using System.Diagnostics;
#endif
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class StringIdGenerator : TypedIdGeneratorBase, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
#if DEBUG_ANALYZER 
        if (!Debugger.IsAttached && false)
        {
            Debugger.Launch();
        }
#endif
        var typedIds = GetAllTypedIds(context.Compilation, "StringIdAttribute");
        foreach (var item in typedIds)
        {
            GenerateStringId(context, item);
        }
    }

    private void GenerateStringId(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var template = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "StringId.cs");

        string resolvedSource = template
            .Replace("% ItemName %", item.Name)
            .Replace("% NamespaceName %", item.ContainingNamespace.ToDisplayString())
            .Replace("% CastOperators %", GenerateCastOperatorsToUnderlyingType(item))
            .Replace("% IsValid %", GetIsValidExpression(item));

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