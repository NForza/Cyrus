using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using NForza.Cyrus.Generators.Roslyn;

public static class CompilationExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypesFromCyrusAssemblies(this Compilation compilation, params string[] assemblySuffixes)
    {
        //caching/memoization could be added here - multiple calls to this method could be expensive
        var compilationTypes = GetTypesFromCompilation(compilation);

        var referencedAssemblyTypes = GetTypesFromReferencedAssemblies(compilation, assemblySuffixes);

        return compilationTypes.Concat(referencedAssemblyTypes).Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>().ToList();
    }

    private static IEnumerable<INamedTypeSymbol> GetTypesFromCompilation(Compilation compilation)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            var typeDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>();
            foreach (var typeDecl in typeDeclarations)
            {
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (typeSymbol != null)
                {
                    yield return typeSymbol;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetTypesFromReferencedAssemblies(Compilation compilation, string[] assemblySuffixes)
    {
        var visitedAssemblies = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.IncludeNullability);
        var assembliesToVisit = new Stack<IAssemblySymbol>();

        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol != null && assemblySymbol.ReferencesCyrusAbstractions() && visitedAssemblies.Add(assemblySymbol))
            {
                assembliesToVisit.Push(assemblySymbol);
            }
        }

        while (assembliesToVisit.Count > 0)
        {
            var currentAssembly = assembliesToVisit.Pop();

            if (assemblySuffixes.Length == 0 || assemblySuffixes.Any(currentAssembly.Name.EndsWith))
            {
                foreach (var type in GetAllTypesFromAssembly(currentAssembly))
                {
                    yield return type;
                }
            }

            foreach (var referencedAssembly in currentAssembly.Modules.SelectMany(m => m.ReferencedAssemblySymbols))
            {
                if (!(referencedAssembly.IsFrameworkAssembly() && visitedAssemblies.Add(referencedAssembly)))
                {
                    assembliesToVisit.Push(referencedAssembly);
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypesFromAssembly(IAssemblySymbol assemblySymbol)
    {
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(assemblySymbol.GlobalNamespace);

        while (stack.Count > 0)
        {
            var namespaceSymbol = stack.Pop();

            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                stack.Push(nestedNamespace);
            }
        }
    }
}
