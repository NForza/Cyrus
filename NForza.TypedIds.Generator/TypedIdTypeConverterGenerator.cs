using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

[Generator]
public class TypedIdTypeConverterGenerator : TypedIdGeneratorBase, ISourceGenerator
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
        var typedIds =
            GetAllTypedIds(context.Compilation, "StringIdAttribute").Concat(GetAllTypedIds(context.Compilation, "GuidIdAttribute"));
        foreach (var item in typedIds)
        {
            GenerateTypeConverter(context, item);
        }
    }


    private void GenerateTypeConverter(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var underlyingTypeName = GetUnderlyingTypeOfTypedId(item)?.ToDisplayString();

        string? source = underlyingTypeName switch
        {
            "System.Guid" => GetGuidConverter(),
            "string" => GetStringConverter(),
            _ => null
        };

        if (source == null)
            return;

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

    private string GetGuidConverter()
    {
        var fileContents = EmbeddedResourceReader.GetResource(Assembly.GetExecutingAssembly(), "Templates", "GuidTypeConverter.cs");
        return fileContents;
    }
}