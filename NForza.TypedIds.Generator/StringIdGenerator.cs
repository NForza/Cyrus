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
        //This will launch the debugger when the generator is running
        //You might have to do a Rebuild to get the generator to run
        if (!Debugger.IsAttached)
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
        public static {item.ToDisplayString()} Empty => new {item.Name}({GetDefault(item)});
        {GenerateIsNullOrEmpty(item)}
        {GenerateCastOperatorsToUnderlyingType(item)}
    }}
}}
");
        context.AddSource($"{item.Name}.g.cs", source.ToString());
    }

    private string GenerateCastOperatorsToUnderlyingType(INamedTypeSymbol item)
    {
        return @$"public static implicit operator {GetUnderlyingTypeOfTypedId(item)?.ToDisplayString()}({item.ToDisplayString()} typedId) => typedId.Value;
        public static explicit operator {item.ToDisplayString()}({GetUnderlyingTypeOfTypedId(item)?.ToDisplayString()} value) => new(value);";
    }

    private object GetDefault(INamedTypeSymbol item)
    {
        return "string.Empty";
    }

    private object GenerateIsNullOrEmpty(INamedTypeSymbol item)
    {
        return $@"public bool IsNullOrEmpty() => string.IsNullOrEmpty(Value);";
    }
}