using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs;

[Generator]
public class CqrsGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
#if DEBUG1 //remove the 1 to enable debugging when compiling source code
        //This will launch the debugger when the generator is running
        //You might have to do a Rebuild to get the generator to run
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
        var additionalFile = context.AdditionalFiles
            .FirstOrDefault(file => Path.GetFileName(file.Path) == "cqrsConfig.txt");
        string configContent = additionalFile.GetText(context.CancellationToken)?.ToString() ?? string.Empty;

        var configuration = ParseConfigFile(configContent);
        IEnumerable<string> contractSuffix = configuration["contracts_projects"]?.Split(',').Select(s => s.Trim()) ?? ["Contracts"];
        var commandSuffix = configuration["command_suffix"] ?? "Command";
        var methodHandlerName = configuration["command_handler_name"] ?? "Execute";

        var commands = GetAllCommandsFromContractsAssemblies(context.Compilation, contractSuffix, commandSuffix).ToList();
        var handlers = context.Compilation
            .GetSymbolsWithName(s => s == methodHandlerName, SymbolFilter.Member)
            .OfType<IMethodSymbol>()
            .Where(m => m.Parameters.Length == 1 && commands.Contains(m.Parameters[0].Type, SymbolEqualityComparer.Default))
            .ToList();

        foreach (var command in commands)
        {
            Debug.WriteLine($"Found command: {command.Name}");
        }

        foreach (var handlerType in handlers.Select(m => m.ContainingType))
        {
            Debug.WriteLine($"Found handler: {handlerType.Name}");
        }

        GenerateCommandProcessorInterface(context, handlers);

        GenerateCommandProcessorImplementation(context, handlers);

        GenerateServiceCollectionExtensions(context, handlers);
    }

    private IEnumerable<INamedTypeSymbol> GetAllCommandsFromContractsAssemblies(Compilation compilation, IEnumerable<string> contractProjectSuffixes, string commandSuffix)
    {
        var commandsInDomain = compilation.GetSymbolsWithName(s => s.EndsWith(commandSuffix), SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .Where(t => t.IsRecord)
            .ToList();

        foreach (var command in commandsInDomain)
        {
            yield return command;
        }

        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol == null
                ||
                assemblySymbol.Name.StartsWith("System")
                ||
                assemblySymbol.Name.StartsWith("Microsoft")
                ||
                !contractProjectSuffixes.Any(assemblySymbol.Name.EndsWith))
                continue;

            // Recursively find all types in the assembly
            var allTypes = GetAllTypes(assemblySymbol.GlobalNamespace);

            // Example: Filter types or perform additional logic
            foreach (var type in allTypes.OfType<INamedTypeSymbol>().Where(t => t.IsRecord && t.Name.EndsWith(commandSuffix)))
            {
                yield return type;
            }
        }
    }

    private Dictionary<string, string> ParseConfigFile(string content)
    {
        var dictionary = new Dictionary<string, string>();
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split('=');
            if (parts.Length == 2)
            {
                dictionary[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return dictionary;
    }

    private IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        // Recursively find all types in the namespace and its nested namespaces
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol nestedNamespace)
            {
                // Recursively get types in the nested namespace
                foreach (var nestedType in GetAllTypes(nestedNamespace))
                {
                    yield return nestedType;
                }
            }
            else if (member is INamedTypeSymbol namedType)
            {
                // Yield the found type
                yield return namedType;
            }
        }
    }


    private void GenerateServiceCollectionExtensions(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new StringBuilder();
        source.Append($@"// <auto-generated/>
using System;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs;

public static class ServiceCollectionExtensions
{{
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {{
        services.AddSingleton<ICommandProcessor, CommandProcessor>();");
        foreach (var typeToRegister in handlers.OfType<IMethodSymbol>().Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default))
        {
            source.Append($@"
        services.AddTransient<{typeToRegister.ToDisplayString()}>();");
        }

        source.Append($@"
        return services;
    }}
}}
");
        context.AddSource($"ServiceCollectionExtensions.g.cs", source.ToString());
    }

    private static void GenerateCommandProcessorImplementation(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new StringBuilder();
        source.Append($@"// <auto-generated/>
using System;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs;

public class CommandProcessor(IServiceProvider serviceProvider): ICommandProcessor
{{");
        foreach (var handler in handlers)
        {
            var methodSymbol = handler;
            var handlerReturnType = methodSymbol.ReturnType;
            var parameterType = methodSymbol.Parameters[0].Type;
            var typeSymbol = methodSymbol.ContainingType;
            source.Append($@"
    public {handlerReturnType.ToDisplayString()} Execute({parameterType} command) 
        => serviceProvider.GetRequiredService<{handler.ContainingType.ToDisplayString()}>().Execute(command);

");
        }

        source.Append($@"
}}
");
        context.AddSource($"CommandProcessor.g.cs", source.ToString());
    }

    private static void GenerateCommandProcessorInterface(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new StringBuilder();
        source.Append($@"// <auto-generated/>
using System;

namespace NForza.Cqrs;

public interface ICommandProcessor
{{");
        foreach (var handler in handlers)
        {
            var methodSymbol = handler as IMethodSymbol;
            var handlerReturnType = methodSymbol.ReturnType;
            var parameterType = methodSymbol.Parameters[0].Type;
            var typeSymbol = methodSymbol.ContainingType;
            source.Append($@"
    {handlerReturnType.ToDisplayString()} Execute({parameterType} command);");
        }
        source.Append($@"
}}
");
        context.AddSource($"ICommandProcessor.g.cs", source.ToString());
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}