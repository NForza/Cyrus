using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.TypedIds.Generator;

[Generator]
public class TypedIdGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG1 //remove the 1 to enable debugging when compiling source code
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
            "System.Guid" => GetGuidConverter(item),
            "string" => GetStringConverter(item),
            _ => null
        };

        if (source == null)
            return;

        source = source.Replace("PLACEHOLDERID", item.Name);
        context.AddSource($"{item}TypeConverter.g.cs", source);
    }

    private string GetStringConverter(INamedTypeSymbol item)
    {
        return $@"
namespace {item.ContainingNamespace};
          
public partial class PLACEHOLDERIDTypeConverter : System.ComponentModel.TypeConverter
{{
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
    {{
        return value switch
        {{
            string str => new CustomerId(Guid.Parse(str)),
            _ => base.ConvertFrom(context, culture, value)
        }};
    }}

    public override object? ConvertTo(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, System.Type destinationType) =>
        destinationType == typeof(string)
            ? ((CustomerId?)value)?.Value.ToString() ?? string.Empty
            : base.ConvertTo(context, culture, value, destinationType);
}}";
    }

    private string GetGuidConverter(INamedTypeSymbol item)
    {
        return $@"
namespace {item.ContainingNamespace};

public partial class PLACEHOLDERIDTypeConverter : System.ComponentModel.TypeConverter
{{
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Type sourceType) 
        => sourceType == typeof(string) || sourceType == typeof(Guid) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
    {{
        return value switch
        {{
            string str => new CustomerId(Guid.Parse(str)),
            Guid guid => new CustomerId(guid),
            _ => base.ConvertFrom(context, culture, value)
        }};
    }}

    public override object? ConvertTo(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, System.Type destinationType) =>
        destinationType == typeof(string)
            ? ((CustomerId?)value)?.Value.ToString() ?? string.Empty
            : base.ConvertTo(context, culture, value, destinationType);
}}";
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

namespace {item.ContainingNamespace}
{{
    public partial record struct {item.Name}
    {{
        {GenerateConstructor(item)}
        public static {item.ToDisplayString()} Empty => new {item.Name}({GetDefault(item)});
        {GenerateIsNullOrEmpty(item)}
    }}
}}
");
        context.AddSource($"{item.Name}.g.cs", source.ToString());
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
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
                if (typeSymbol != null && typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct)
                {
                    if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "TypedIdAttribute"))
                        allTypes.Add(typeSymbol);
                }
            }
        }

        return allTypes;
    }

}
