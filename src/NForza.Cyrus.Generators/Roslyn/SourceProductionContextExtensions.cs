using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn;

internal static class SourceProductionContextExtensions
{
    public static void AddSource(this SourceProductionContext context, string hintName, string sourceText)
        => context.AddSource(hintName, sourceText);
}
