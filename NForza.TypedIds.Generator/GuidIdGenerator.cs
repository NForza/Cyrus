﻿using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

namespace NForza.TypedIds.Generator;

[Generator]
public class GuidIdGenerator : TypedIdGeneratorBase, ISourceGenerator
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
        var typedIds = GetAllTypedIds(context.Compilation, "GuidIdAttribute");
        foreach (var item in typedIds)
        {
            GenerateGuidId(context, item);
        }
    }

    private void GenerateGuidId(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var source = new StringBuilder($@"
using System;
using NForza.TypedIds;
using System.Text.Json.Serialization;

namespace {item.ContainingNamespace}
{{
    [JsonConverter(typeof({item.Name}JsonConverter))]
    public partial record struct {item.Name}(Guid Value): ITypedId
    {{
        {GenerateConstructor(item)}
        public static {item.ToDisplayString()} Empty => new {item.Name}({GetDefault(item)});
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
        return "Guid.Empty";
    }

    string GenerateConstructor(INamedTypeSymbol item)
    {
        return $@"public {item.Name}(): this(Guid.NewGuid()) {{}}";
    }
}