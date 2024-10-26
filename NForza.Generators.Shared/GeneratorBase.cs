using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

#pragma warning disable RS1035 // Do not use APIs banned for analyzers

namespace NForza.Generators
{
    public abstract class GeneratorBase : ISourceGenerator
    {
        protected T ParseConfigFile<T>(GeneratorExecutionContext context, string configFileName)
            where T : IYamlConfig<T>, new()
        {
            var additionalFile = context.AdditionalFiles
                .FirstOrDefault(file => Path.GetFileName(file.Path) == configFileName);
            string configContent = additionalFile?.GetText(context.CancellationToken)?.ToString() ?? string.Empty;
            var config = YamlParser.ReadYaml(configContent);
            return new T().InitFrom(config);
        }

        protected IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
        {
            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (member is INamespaceSymbol nestedNamespace)
                {
                    foreach (var nestedType in GetAllTypes(nestedNamespace))
                    {
                        yield return nestedType;
                    }
                }
                else if (member is INamedTypeSymbol namedType)
                {
                    yield return namedType;
                }
            }
        }

        public virtual void Initialize(GeneratorInitializationContext context) { }

        public abstract void Execute(GeneratorExecutionContext context);
    }
}
