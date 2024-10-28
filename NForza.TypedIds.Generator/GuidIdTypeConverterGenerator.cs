using System.Collections.Generic;
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
        var replacements = new Dictionary<string, string>
        {
            ["TypedIdName"] = item.Name,
            ["NamespaceName"] = item.ContainingNamespace.ToDisplayString()
        };

        string source = TemplateEngine.ReplaceInResourceTemplate("GuidTypeConverter.cs", replacements);

        context.AddSource($"{item}TypeConverter.g.cs", source);
    }
}