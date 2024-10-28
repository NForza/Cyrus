using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

public abstract class TypedIdGeneratorBase : GeneratorBase, ISourceGenerator
{
    protected IEnumerable<INamedTypeSymbol> GetAllTypedIds(Compilation compilation, string typedIdName)
    {
        var allTypes = new List<INamedTypeSymbol>();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var typeDeclarations = syntaxTree.GetRoot().DescendantNodes()
                .OfType<TypeDeclarationSyntax>();

            foreach (var typeDeclaration in typeDeclarations)
            {
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is INamedTypeSymbol typeSymbol && typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct)
                {
                    if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == typedIdName))
                        allTypes.Add(typeSymbol);
                }
            }
        }

        return allTypes;
    }

    protected string GetUnderlyingTypeOfTypedId(INamedTypeSymbol typeSymbol)
    {
        var hasStringIdProperty = typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "StringIdAttribute");
        if (hasStringIdProperty)
            return "string";
        return "System.Guid";
    }
}
