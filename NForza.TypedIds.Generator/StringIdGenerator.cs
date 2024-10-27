using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

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
        var source = new StringBuilder($@"
using System;
using NForza.TypedIds;
using System.Text.Json.Serialization;

namespace {item.ContainingNamespace}
{{
    [JsonConverter(typeof({item.Name}JsonConverter))]
    public partial record struct {item.Name}(string Value): ITypedId
    {{
        public static {item.ToDisplayString()} Empty => new {item.Name}({GetDefault()});
        {GenerateIsNullOrEmpty()}
        {GenerateCastOperatorsToUnderlyingType(item)}
    }}
}}
");
        string resolvedSource = source.ToString();
        context.AddSource($"{item.Name}.g.cs", resolvedSource);
    }

    private string GenerateCastOperatorsToUnderlyingType(INamedTypeSymbol item)
    {
        return @$"public static implicit operator {GetUnderlyingTypeOfTypedId(item)}({item.ToDisplayString()} typedId) => typedId.Value;
        public static explicit operator {item.ToDisplayString()}({GetUnderlyingTypeOfTypedId(item)} value) => new(value);";
    }

    private object GetDefault()
    {
        return "string.Empty";
    }

    private object GenerateIsNullOrEmpty()
    {
        return $@"public bool IsNullOrEmpty() => string.IsNullOrEmpty(Value);";
    }
}