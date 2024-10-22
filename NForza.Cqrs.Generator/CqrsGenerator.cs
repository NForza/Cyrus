using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

#pragma warning disable RS1035 // Do not use banned APIs for analyzers

namespace NForza.Cqrs.Generator;

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
            .FirstOrDefault(file => Path.GetFileName(file.Path) == "cqrsConfig.yaml");
        string configContent = additionalFile.GetText(context.CancellationToken)?.ToString() ?? string.Empty;

        var configuration = ParseConfigFile(configContent);
        IEnumerable<string> contractSuffix = configuration.Contracts;
        var commandSuffix = configuration.Commands.Suffix;
        var methodHandlerName = configuration.Commands.HandlerName;

        var commands = GetAllCommandsFromContractsAssemblies(context.Compilation, contractSuffix, commandSuffix).ToList();
        VerifyCommands(context, commands);

        var handlers = context.Compilation
            .GetSymbolsWithName(s => s == methodHandlerName, SymbolFilter.Member)
            .OfType<IMethodSymbol>()
            .Where(m => m.Parameters.Length == 1 && commands.Contains(m.Parameters[0].Type, SymbolEqualityComparer.Default))
            .Where(m => ReturnsTaskOfCommandResult(context, m))
            .ToList();

        GenerateCommandDispatcherExtensionMethods(context, handlers);

        GenerateServiceCollectionExtensions(context, handlers);
    }

    private static bool ReturnsTaskOfCommandResult(GeneratorExecutionContext context, IMethodSymbol handlerType)
    {
        Debug.WriteLine($"Found handler: {handlerType.Name}");

        if (IsTaskOfCommandResult(handlerType.ReturnType, context.Compilation))
        {
            return true;
        }
        else
        {
            context.ReportDiagnostic(Diagnostic.Create(
                 new DiagnosticDescriptor(
                     "SG0001",
                     "Incorrect return type for command handler",
                     "Method {0} returns {1}. All methods must return Task<CommandResult>.",
                     "NForza.Cqrs",
                     DiagnosticSeverity.Error,
                     true), handlerType.Locations.FirstOrDefault(), handlerType.Name, handlerType.ReturnType.Name));
            return false;
        }
    }

    private static bool IsTaskOfCommandResult(ITypeSymbol returnType, Compilation compilation)
    {
        // Check if the return type is a Task
        if (returnType is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsGenericType &&
                    namedTypeSymbol.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            {

                var genericArguments = namedTypeSymbol.TypeArguments;
                if (genericArguments.Length == 1)
                {
                    var commandResultSymbol = compilation.GetTypeByMetadataName("NForza.Cqrs.CommandResult");
                    return SymbolEqualityComparer.Default.Equals(genericArguments[0], commandResultSymbol);
                }
            }// Get the generic argument
        }
        return false;
    }

    private static void VerifyCommands(GeneratorExecutionContext context, List<INamedTypeSymbol> commands)
    {
        foreach (var command in commands)
        {
            Debug.WriteLine($"Found command: {command.Name}");
        }
    }

    private IEnumerable<INamedTypeSymbol> GetAllCommandsFromContractsAssemblies(Compilation compilation, IEnumerable<string> contractProjectSuffixes, string commandSuffix)
    {
        bool IsStruct(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct;
        }

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
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol
                ||
                assemblySymbol.Name.StartsWith("System")
                ||
                assemblySymbol.Name.StartsWith("Microsoft")
                ||
                !contractProjectSuffixes.Any(assemblySymbol.Name.EndsWith))
                continue;

            var allTypes = GetAllTypes(assemblySymbol.GlobalNamespace);

            foreach (var t in allTypes)
            {
                if (IsStruct(t))
                    yield return t;
            }
        }
    }

    private CqrsConfig ParseConfigFile(string content)
    {
        var config = YamlParser.ReadYaml(content);
        return new CqrsConfig(config);
    }

    private IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
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


    private void GenerateServiceCollectionExtensions(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        source.Append($@"// <auto-generated/>
using System;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs;

public static class ServiceCollectionExtensions
{{
    public static IServiceCollection AddCqrs(this IServiceCollection services, Action<CqrsOptions>? options = null)
    {{
        options?.Invoke(new CqrsOptions(services));
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<ICommandBus, LocalCommandBus>(); 
        services.AddSingleton<IEventBus, EventBus>(); 
        services.AddSingleton(BuildHandlerDictionary());");
        foreach (var typeToRegister in handlers.Select(h => h.ContainingType).Distinct(SymbolEqualityComparer.Default))
        {
            source.Append($@"
        services.AddTransient<{typeToRegister.ToDisplayString()}>();");
        }


        source.Append($@"
        return services;
    }}

    public static HandlerDictionary BuildHandlerDictionary()
    {{
        var handlers = new HandlerDictionary();");
        foreach (var handler in handlers)
        {
            var handlerReturnType = handler.ReturnType;
            var commandType = handler.Parameters[0].Type;
            var typeSymbol = handler.ContainingType;

            source.Append($@"
        handlers.AddHandler<{commandType}>((services, command) => services.GetRequiredService<{typeSymbol}>().Execute(({commandType})command));");
        }
        source.Append($@"
        return handlers;
    }}
}}");
        context.AddSource($"ServiceCollectionExtensions.g.cs", source.ToString());
    }

    private static void GenerateCommandDispatcherExtensionMethods(GeneratorExecutionContext context, List<IMethodSymbol> handlers)
    {
        StringBuilder source = new();
        source.Append($@"// <auto-generated/>
using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs;

public static class CommandDispatcherExtensions
{{");
        foreach (var handler in handlers)
        {
            var methodSymbol = handler;
            var parameterType = methodSymbol.Parameters[0].Type;
            source.Append($@"
    public static Task<CommandResult> Execute(this ICommandDispatcher dispatcher, {parameterType} command, CancellationToken cancellationToken = default) 
        => dispatcher.ExecuteInternal(command, cancellationToken);");
        }

        source.Append($@"
}}
");
        context.AddSource($"CommandProcessor.g.cs", source.ToString());
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}