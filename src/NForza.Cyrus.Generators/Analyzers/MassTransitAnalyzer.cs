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

       if (methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is ConstructorDeclarationSyntax ctorSyntax
            && ctorSyntax.Body != null)
        {
            (bool shouldUseMassTransitAssembly, Location location) = CallsUseMassTransit(methodSymbol, ctorSyntax);
            if (!shouldUseMassTransitAssembly)
            {
                return;
            }
            var referencesCorrectAssembly =
                ReferencesAssembly(context.Compilation, "NForza.Cyrus.MassTransit");

            if (!referencesCorrectAssembly)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.ProjectShouldReferenceCyrusMassTransit,
                    location,
                    methodSymbol.ToDisplayString());
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }
    }

    private (bool, Location) CallsUseMassTransit(IMethodSymbol ctorSymbol, ConstructorDeclarationSyntax constructorDeclarationSyntax)
    {
        foreach (var invocation in constructorDeclarationSyntax.Body!.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var identifier = (invocation.Expression as IdentifierNameSyntax)?.Identifier.Text;
            if (identifier == "UseMassTransit") 
                return (true, invocation.GetLocation());
        }
        return (false, Location.None);
    }
}
