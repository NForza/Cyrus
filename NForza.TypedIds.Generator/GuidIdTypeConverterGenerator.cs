using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class GuidIdTypeConverterGenerator : TypedIdGeneratorBase, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
#if DEBUG_ANALYZER 
        if (!Debugger.IsAttached && false)
        {
            Debugger.Launch();
        }
#endif
        var typedIds = GetAllTypedIds(context.Compilation, "GuidIdAttribute");
        foreach (var item in typedIds)
        {
            GenerateGuidIdTypeConverter(context, item);
        }
    }

    private void GenerateGuidIdTypeConverter(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        string source = GetGuidConverter();

        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        source = source
            .Replace("% TypedIdName %", item.Name)
            .Replace("% NamespaceName %", fullyQualifiedNamespace);
        context.AddSource($"{item}TypeConverter.g.cs", source);
    }

    private string GetGuidConverter()
    {
        var fileContents = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "GuidTypeConverter.cs");
        return fileContents;
    }
}