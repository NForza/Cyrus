using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class StringIdTypeConverterGenerator : TypedIdGeneratorBase, ISourceGenerator
{
    public override void Execute(GeneratorExecutionContext context)
    {
#if DEBUG_ANALYZER 
        if (!Debugger.IsAttached && false)
        {
            Debugger.Launch();
        }
#endif
        var stringIds =
            GetAllTypedIds(context.Compilation, "StringIdAttribute");
        foreach (var item in stringIds)
        {
            GenerateStringIdTypeConverter(context, item);
        }
    }


    private void GenerateStringIdTypeConverter(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        string source = GetStringConverter();

        string fullyQualifiedNamespace = item.ContainingNamespace.ToDisplayString();
        source = source
            .Replace("% TypedIdName %", item.Name)
            .Replace("% NamespaceName %", fullyQualifiedNamespace);
        context.AddSource($"{item}TypeConverter.g.cs", source);
    }

    private string GetStringConverter()
    {
        var fileContents = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "StringTypeConverter.cs");
        return fileContents;
    }
}