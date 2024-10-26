using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class TypedIdJsonConverterGenerator : TypedIdGeneratorBase, ISourceGenerator
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
            GenerateJsonConverter(context, item);
        }
    }

    private void GenerateJsonConverter(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var source = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "JsonConverter.cs");

        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        var underlyingTypeName = GetUnderlyingTypeOfTypedId(item)?.ToDisplayString();

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