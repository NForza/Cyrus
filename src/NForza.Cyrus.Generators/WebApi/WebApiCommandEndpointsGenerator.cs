using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.WebApi;

[Generator]
public class CommandEndpointsGenerator : CyrusSourceGeneratorBase, IIncrementalGenerator
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugThisGenerator(true);

        var configProvider = ConfigProvider(context);

        var compilationProvider = context.CompilationProvider;

        var typesFromReferencedAssemblyProvider = compilationProvider
            .SelectMany((compilation, _) =>
            {
                var typesFromAssemblies = compilation.GetAllTypesFromCyrusAssemblies();

                var commandsFromAssemblies = typesFromAssemblies
                    .Where(t => t.IsCommand())
                    .ToList();

                var commandHandlersFromAssemblies = typesFromAssemblies.SelectMany(t => t.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsCommandHandler()));

                var csharpCompilation = (CSharpCompilation)compilation;
                var typesInCompilation = csharpCompilation.GetAllTypesFromCyrusAssemblies();
                var commandsInCompilation = typesInCompilation.Where(t => t.IsCommand() )
                    .ToList();
                var commandHandlersInCompilation = typesInCompilation.SelectMany(t => t.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsCommandHandler()));

                var allCommands = typesFromAssemblies
                    .Union(typesInCompilation, SymbolEqualityComparer.Default)
                    .OfType<INamedTypeSymbol>()
                    .Where(t => t.IsCommand())
                    .Cast<ISymbol>();
                var allCommandHandlers = commandHandlersFromAssemblies
                    .Union(commandHandlersInCompilation, SymbolEqualityComparer.Default)
                    .Cast<ISymbol>();
                return allCommands.Cast<ISymbol>().Concat(allCommandHandlers);
            })
           .Collect();

        var serviceCollectionCombinedProvider = context
            .CompilationProvider
            .Combine(typesFromReferencedAssemblyProvider)
            .Combine(configProvider);

        context.RegisterSourceOutput(serviceCollectionCombinedProvider, (sourceProductionContext, source) =>
        {
            var ((compilation, typesFromReferencedAssemblies), config) = source;

            if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
            {
                IEnumerable<IMethodSymbol> handlers = typesFromReferencedAssemblies.OfType<IMethodSymbol>().ToList();
                IEnumerable<INamedTypeSymbol> commands = typesFromReferencedAssemblies.OfType<INamedTypeSymbol>().ToList();

                var contents = AddCommandHandlerMappings(sourceProductionContext, handlers, compilation);

                if (!string.IsNullOrWhiteSpace(contents))
                {
                    var ctx = new
                    {
                        Usings = new string[] {
                            "Microsoft.AspNetCore.Mvc",
                            "Microsoft.AspNetCore.Http"
                    },
                        Namespace = "WebApiCommands",
                        Name = "Command",
                        StartupCommands = contents
                    };

                    var fileContents = LiquidEngine.Render(ctx, "CyrusWebStartup");
                    sourceProductionContext.AddSource(
                       "CommandHandlerMapping.g.cs",
                       SourceText.From(fileContents, Encoding.UTF8));
                }
                AddHttpContextObjectFactoryMethodsRegistrations(sourceProductionContext, commands);

                WebApiContractGenerator.GenerateCommandContracts(handlers, sourceProductionContext, LiquidEngine);
            }
        });
    }

    private void AddHttpContextObjectFactoryMethodsRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<INamedTypeSymbol> queries)
    {
        var model = new
        {
            Commands = queries.Select(cmd =>
                new
                {
                    Name = cmd.Name,
                    TypeName = cmd.ToFullName(),
                    Properties = cmd.GetPublicProperties().Select(p => new { Name = p.Name, Type = p.Type.ToFullName() })
                })
        };
        var httpContextObjectFactoryInitialization = LiquidEngine.Render(model, "HttpContextObjectFactoryCommand");

        var initModel = new { Namespace = "WebApi", Name = "HttpContextObjectFactoryCommandInitializer", Initializer = httpContextObjectFactoryInitialization };
        var source = LiquidEngine.Render(initModel, "CyrusInitializer");
        sourceProductionContext.AddSource($"HttpContextObjectFactoryCommands.g.cs", SourceText.From(source, Encoding.UTF8));
    }


    private string AddCommandHandlerMappings(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers, Compilation compilation)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var handler in handlers)
        {
            var command = new
            {
                Path = handler.GetCommandHandlerRoute(),
                Verb = handler.GetCommandHandlerVerb(),
                HasBody = handler.HasCommandBody(),
                CommandType = handler.Parameters[0].Type.ToFullName(),
                CommandName = handler.Parameters[0].Type.Name,
                AdapterMethod = GetAdapterMethodName(handler),
                IsAsync = handler.IsAsync,
                HasReturnType = handler.ReturnType.SpecialType != SpecialType.System_Void,
                CommandInvocation = handler.GetCommandInvocation(variableName: "cmd")
            };
            
            sb.AppendLine(LiquidEngine.Render(command, "MapCommand"));
        }
        return sb.ToString().Trim();
    }

    private string GetAdapterMethodName(IMethodSymbol handler)
    {
        var returnType = handler.ReturnType;
        if (returnType.SpecialType == SpecialType.System_Void)
            return "FromVoid";
        if (returnType.IsTupleType)
        {
            return "FromIResultAndEvents";
        }
        return "FromObjects";
    }
}