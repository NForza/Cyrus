using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NForza.Cyrus.Generators.Roslyn;

internal static class SourceProductionContextExtensions
{
    public static void AddSource(this SourceProductionContext context, string hintName, string sourceText) 
        => context.AddSource(hintName, sourceText);
}
