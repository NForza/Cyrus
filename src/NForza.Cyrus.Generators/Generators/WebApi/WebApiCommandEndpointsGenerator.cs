using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.Generators.WebApi;
public class WebApiCommandEndpointsGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider, LiquidEngine liquidEngine)
    {
        var config = cyrusProvider.GenerationConfig;
        if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            IEnumerable<IMethodSymbol> handlers = cyrusProvider.AllCommandsAndHandlers.OfType<IMethodSymbol>().ToList();
            IEnumerable<INamedTypeSymbol> commands = cyrusProvider.AllCommandsAndHandlers.OfType<INamedTypeSymbol>().ToList();

            var contents = AddCommandHandlerMappings(spc, handlers, cyrusProvider.Compilation, liquidEngine);

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
                spc.AddSource(
                   "CommandHandlerMapping.g.cs",
                   SourceText.From(fileContents, Encoding.UTF8));
            }
            AddHttpContextObjectFactoryMethodsRegistrations(spc, commands, liquidEngine);

            WebApiContractGenerator.GenerateCommandContracts(handlers, spc, liquidEngine);
        }
    }

    private void AddHttpContextObjectFactoryMethodsRegistrations(SourceProductionContext sourceProductionContext, IEnumerable<INamedTypeSymbol> queries, LiquidEngine liquidEngine)
    {
        var model = new
        {
            Commands = queries.Select(cmd =>
                new
                {
                    cmd.Name,
                    TypeName = cmd.ToFullName(),
                    Properties = cmd.GetPublicProperties().Select(p => new { p.Name, Type = p.Type.ToFullName() })
                })
        };
        var httpContextObjectFactoryInitialization = LiquidEngine.Render(model, "HttpContextObjectFactoryCommand");

        var initModel = new { Namespace = "WebApi", Name = "HttpContextObjectFactoryCommandInitializer", Initializer = httpContextObjectFactoryInitialization };
        var source = liquidEngine.Render(initModel, "CyrusInitializer");
        sourceProductionContext.AddSource($"HttpContextObjectFactoryCommands.g.cs", SourceText.From(source, Encoding.UTF8));
    }


    private string AddCommandHandlerMappings(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers, Compilation compilation, LiquidEngine liquidEngine)
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
                ReturnsTask =  handler.ReturnsTask(),
                HasReturnType = handler.ReturnType.SpecialType != SpecialType.System_Void && !(handler.ReturnsTask() && handler.TypeArguments.Any()),
                CommandInvocation = handler.GetCommandInvocation(variableName: "cmd")
            };

            sb.AppendLine(liquidEngine.Render(command, "MapCommand"));
        }
        return sb.ToString().Trim();
    }

    private string GetAdapterMethodName(IMethodSymbol handler)
    {
        var returnType = handler switch
        {
            _ when handler.ReturnsTask(out var taskResultType) && taskResultType != null => taskResultType,
            _ when handler.ReturnsTask() => null,
            _ => handler.ReturnType
        };
        
        if (returnType == null || returnType.SpecialType == SpecialType.System_Void)
            return "FromVoid";

        if (returnType.IsTupleType)
        {
            return "FromIResultAndEvents";
        }
        return "FromObjects";
    }
}