using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NForza.Cyrus.Generators.Analyzers;

internal class MassTransitAnalyzer : CyrusAnalyzerBase
{
    public override void AnalyzeMethodSymbol(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        bool IsDirectlyInheritingFrom(IMethodSymbol methodSymbol, string expectedBaseClassName)
        {
            var containingType = methodSymbol.ContainingType;
            var baseType = containingType.BaseType;

            return baseType != null &&
                   baseType.ToDisplayString() == expectedBaseClassName;
        }

        bool ReferencesAssembly(Compilation compilation, string assemblyName)
        {
            return compilation.ReferencedAssemblyNames
                .Any(r => r.Name == assemblyName);
        }

        if (methodSymbol.Name != ".ctor")
            return;

        if (!IsDirectlyInheritingFrom(methodSymbol, "NForza.Cyrus.Abstractions.CyrusConfig"))
        {
            return;
        }
    }
}
