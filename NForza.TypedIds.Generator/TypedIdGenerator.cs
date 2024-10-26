using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

namespace NForza.TypedIds.Generator;

[Generator]
public class TypedIdGenerator : TypedIdGeneratorBase, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
#if DEBUG_ANALYZER //remove the 1 to enable debugging when compiling source code
        //This will launch the debugger when the generator is running
        //You might have to do a Rebuild to get the generator to run
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
        var typedIds = GetAllTypedIds(context.Compilation);
        foreach (var item in typedIds)
        {
            GenerateTypedId(context, item);
        }
    }

    private void GenerateTypedId(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var source = new StringBuilder($@"
using System;
using NForza.TypedIds;
using System.Text.Json.Serialization;

namespace {item.ContainingNamespace}
{{
    [JsonConverter(typeof({item.Name}JsonConverter))]
    public partial record struct {item.Name}: ITypedId
    {{
        {GenerateConstructor(item)}
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
        var underlyingType = GetUnderlyingTypeOfTypedId(item);
        if (underlyingType?.ToDisplayString() == "string")
        {
            return "string.Empty";
        }
        if (underlyingType?.ToDisplayString() == "System.Guid")
        {
            return "Guid.Empty";
        }
        return "default";
    }

    string GenerateConstructor(INamedTypeSymbol item)
    {
        ITypeSymbol? underlyingType = GetUnderlyingTypeOfTypedId(item);
        if (underlyingType?.ToDisplayString() == "System.Guid")
        {
            return $@"public {item.Name}(): this(Guid.NewGuid()) {{}}";
        }
        return string.Empty;
    }

    private object GenerateIsNullOrEmpty(INamedTypeSymbol item)
    {
        ITypeSymbol? underlyingType = GetUnderlyingTypeOfTypedId(item);
        if (underlyingType?.ToDisplayString() == "string")
        {
            return $@"public bool IsNullOrEmpty() => string.IsNullOrEmpty(Value);";
        }
        return string.Empty;
    }
}