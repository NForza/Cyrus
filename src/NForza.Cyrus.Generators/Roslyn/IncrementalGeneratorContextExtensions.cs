using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NForza.Cyrus.Generators.Roslyn
{
    public static class IncrementalGeneratorContextExtensions
    {
        /// <summary>
        /// Returns an IncrementalValueProvider that is true if this is a test project (via MSBuild or test framework reference) or IsTestProject build property
        /// </summary>
        public static IncrementalValueProvider<bool> CodeGenerationSuppressed(this IncrementalGeneratorInitializationContext context)
        {
            return context.CompilationProvider
                .Combine(context.AnalyzerConfigOptionsProvider)
                .Select((tuple, _) =>
                {
                    var (compilation, options) = tuple;

                    if (options.GlobalOptions.TryGetValue("build_property.EnableCyrusGeneration", out var isEnabled) &&
                        isEnabled.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    if (options.GlobalOptions.TryGetValue("build_property.IsTestProject", out var isTest) &&
                        isTest.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    var testRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "xunit",
                        "nunit",
                        "Microsoft.VisualStudio.TestPlatform.TestFramework"
                    };

                    return compilation.ReferencedAssemblyNames
                            .Any(a => testRefs.Any(prefix => a.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
                });
        }
    }
}
