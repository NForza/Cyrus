using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NForza.TypedIds.Generator;

[Generator]
public class TypedIdGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG//remove the 1 to enable debugging when compiling source code
        //This will launch the debugger when the generator is running
        //You might have to do a Rebuild to get the generator to run
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
        var typedIds = GetAllTypes(context.Compilation);
        foreach (var item in typedIds)
        {
            GenerateTypedId(context, item);
            GenerateTypeConverter(context, item);
            GenerateJsonConverter(context, item);
        }
    }

    private void GenerateJsonConverter(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
    }

    private void GenerateTypeConverter(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var underlyingTypeName = GetUnderlyingType(item)?.ToDisplayString();

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
            .Replace("%TypedIdName%", item.Name)
            .Replace("%NamespaceName%", fullyQualifiedNamespace);
        context.AddSource($"{item}TypeConverter.g.cs", source);
    }

    private string GetStringConverter()
    {
        var fileContents = EmbeddedResourceReader.GetResource("Templates", "StringTypeConverter.cs");
        return fileContents;
    }

    private string GetGuidConverter()
    {
        var fileContents = EmbeddedResourceReader.GetResource("Templates", "GuidTypeConverter.cs");
        return fileContents;
    }

    ITypeSymbol? GetUnderlyingType(INamedTypeSymbol typeSymbol)
    {
        var firstProperty = typeSymbol.GetMembers()
              .OfType<IPropertySymbol>()
              .FirstOrDefault();
        return firstProperty?.Type;
    }

    private void GenerateTypedId(GeneratorExecutionContext context, INamedTypeSymbol item)
    {
        var source = new StringBuilder($@"
using System;
using NForza.TypedIds;

namespace {item.ContainingNamespace}
{{
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
        return @$"public static implicit operator {GetUnderlyingType(item)?.ToDisplayString()}({item.ToDisplayString()} typedId) => typedId.Value;
        public static explicit operator {item.ToDisplayString()}({GetUnderlyingType(item)?.ToDisplayString()} value) => new(value);";
    }

    private object GetDefault(INamedTypeSymbol item)
    {
        var underlyingType = GetUnderlyingType(item);
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
        ITypeSymbol? underlyingType = GetUnderlyingType(item);
        if (underlyingType?.ToDisplayString() == "System.Guid")
        {
            return $@"public {item.Name}(): this(Guid.NewGuid()) {{}}";
        }
        return string.Empty;
    }

    private object GenerateIsNullOrEmpty(INamedTypeSymbol item)
    {
        ITypeSymbol? underlyingType = GetUnderlyingType(item);
        if (underlyingType?.ToDisplayString() == "string")
        {
            return $@"public bool IsNullOrEmpty() => string.IsNullOrEmpty(Value);";
        }
        return string.Empty;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
    {
        var allTypes = new List<INamedTypeSymbol>();

        // Traverse each syntax tree in the compilation
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var typeDeclarations = syntaxTree.GetRoot().DescendantNodes()
                .OfType<TypeDeclarationSyntax>();

            foreach (var typeDeclaration in typeDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol typeSymbol && typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct)
                {
                    if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "TypedIdAttribute"))
                        allTypes.Add(typeSymbol);
                }
            }
        }

        return allTypes;
    }

}