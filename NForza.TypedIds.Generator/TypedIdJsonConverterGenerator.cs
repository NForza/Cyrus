using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        var source = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "JsonConverter.cs");

        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        var underlyingTypeName = GetUnderlyingTypeOfTypedId(item);

        string? getMethodName = underlyingTypeName switch
        {
            "System.Guid" => "GetGuid",
            "string" => "GetString",
            _ => null
        };

        source = source
            .Replace("% TypedIdName %", item.Name)
            .Replace("% NamespaceName %", fullyQualifiedNamespace)
            .Replace("% GetMethodName %", getMethodName);
        context.AddSource($"{item}JsonConverter.g.cs", source);
    }
}