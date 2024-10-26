using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NForza.Generators;

namespace NForza.TypedIds.Generator;

public abstract class TypedIdGeneratorBase : GeneratorBase, ISourceGenerator
{

    protected IEnumerable<INamedTypeSymbol> GetAllTypedIds(Compilation compilation, string typedIdName)
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
                    if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == typedIdName))
                        allTypes.Add(typeSymbol);
                }
            }
        }

        return allTypes;
    }

    protected ITypeSymbol? GetUnderlyingTypeOfTypedId(INamedTypeSymbol typeSymbol)
    {
        var firstProperty = typeSymbol.GetMembers()
              .OfType<IPropertySymbol>()
              .FirstOrDefault();
        return firstProperty?.Type;
    }
}
