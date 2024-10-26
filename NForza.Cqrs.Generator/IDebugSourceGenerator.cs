using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

#pragma warning disable RS1035 // Do not use APIs banned for analyzers

namespace NForza.Cqrs.Generator
{
    public class DebugSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(
             new DiagnosticDescriptor(
                 "GEN001",
                 "Source Generator Message",
                 "Hello from the source generator!",
                 "SourceGeneration",
                 DiagnosticSeverity.Info,
                 true),
             Location.None));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Debug.WriteLine("DebugSourceGenerator.Initialize");
        }
    }
}
