using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class TypedIdJsonConverterGenerator : TypedIdGeneratorBase, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
#if DEBUG_ANALYZER 
        if (!Debugger.IsAttached && false)
        {
            Debugger.Launch();
        }
#endif
        var typedIds = 
            GetAllTypedIds(context.Compilation, "StringIdAttribute").Concat(GetAllTypedIds(context.Compilation, "GuidIdAttribute"));
        foreach (var item in typedIds)
        {
            GenerateJsonConverter(context, item);
        }
    }

    private void GenerateJsonConverter(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        var underlyingTypeName = GetUnderlyingTypeOfTypedId(item);

        string? getMethodName = underlyingTypeName switch
        {
            "System.Guid" => "GetGuid",
            "string" => "GetString",
            _ => throw new NotSupportedException($"Underlying type {underlyingTypeName} is not supported.")
        };

        var replacements = new Dictionary<string, string>
        {
            ["TypedIdName"] = item.Name,
            ["NamespaceName"] = fullyQualifiedNamespace,
            ["GetMethodName"] = getMethodName
        };
        var source = TemplateEngine.ReplaceInResourceTemplate("JsonConverter.cs", replacements);
        context.AddSource($"{item}JsonConverter.g.cs", source);
    }
}