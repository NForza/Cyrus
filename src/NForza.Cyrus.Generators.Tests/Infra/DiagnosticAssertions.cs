using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Tests.Infra;

public static class DiagnosticAssertions
{
    public static void NotHaveErrors(this GenericCollectionAssertions<Diagnostic> diagnostics)
    {
        diagnostics.NotContain(d => d.Severity == DiagnosticSeverity.Error, "Diagnostics should not contain errors");
    }

    public static void HaveErrorCount(this GenericCollectionAssertions<Diagnostic> diagnostics, int errorCount)
    {
        diagnostics.Subject.Count(d => d.Severity == DiagnosticSeverity.Error).Should().Be(errorCount,
            $"Diagnostics should contain {errorCount} errors");
    }

    public static void ContainDiagnostic(this GenericCollectionAssertions<Diagnostic> diagnostics, DiagnosticDescriptor diagnosticDescriptor)
    {
        diagnostics.Subject.Should().Contain(c => c.Descriptor == diagnosticDescriptor, $"Diagnostics should contain '{diagnosticDescriptor.Description}'.");
    }

    public static void ContainError(this GenericCollectionAssertions<Diagnostic> diagnostics, DiagnosticDescriptor diagnosticDescriptor)
    {
        diagnostics.Subject.Should().Contain(c => c.Severity == DiagnosticSeverity.Error && c.Descriptor == diagnosticDescriptor, $"Diagnostics should contain '{diagnosticDescriptor.Description}'.");
    }
}
