using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.WebApi;
public class WebApiCommandEndpointsGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusGenerationContext)
    {
        var config = cyrusGenerationContext.GenerationConfig;
        if (config != null && config.GenerationTarget.Contains(GenerationTarget.WebApi))
        {
            IEnumerable<IMethodSymbol> handlers =
                cyrusGenerationContext
                    .All.CommandHandlers
                    .OfType<IMethodSymbol>()
                    .ToList();

            IEnumerable<INamedTypeSymbol> commands = cyrusGenerationContext.All.Commands.OfType<INamedTypeSymbol>().ToList();
            IEnumerable<IMethodSymbol> validators = cyrusGenerationContext.Validators;

            var contents = AddCommandHandlerMappings(spc, handlers.Where(h => h.HasCommandRoute()), validators, cyrusGenerationContext.Compilation, cyrusGenerationContext.LiquidEngine);

            if (!string.IsNullOrWhiteSpace(contents))
            {
                var ctx = new
                {
                    Usings = new string[] {
                        "System.Linq",
                        "Microsoft.AspNetCore.Builder",
                        "Microsoft.AspNetCore.Mvc",
                        "Microsoft.AspNetCore.Http",
                        "Microsoft.Extensions.DependencyInjection"
                    },
                    Namespace = "WebApiCommands",
                    Name = "Command",
                    StartupCommands = contents
                };

                var fileContents = cyrusGenerationContext.LiquidEngine.Render(ctx, "CyrusWebStartup");
                spc.AddSource("CommandHandlerMapping.g.cs", fileContents);
            }

            AddHttpContextObjectFactoryMethodsRegistrations(commands, spc, cyrusGenerationContext.LiquidEngine);

            WebApiContractGenerator.GenerateCommandContracts(handlers, spc, cyrusGenerationContext.LiquidEngine);
        }
    }

    private void AddHttpContextObjectFactoryMethodsRegistrations(IEnumerable<INamedTypeSymbol> commands, SourceProductionContext sourceProductionContext, LiquidEngine liquidEngine)
    {
        var model = new
        {
            Commands = commands.Select(cmd =>
                new
                {
                    cmd.Name,
                    TypeName = cmd.ToFullName(),
                    Properties = cmd.GetPublicProperties().Select(p => new { p.Name, Type = p.Type.ToFullName() })
                })
        };
        var httpContextObjectFactoryInitialization = liquidEngine.Render(model, "HttpContextObjectFactoryCommand");

        var initModel = new
        {
            Namespace = "WebApi",
            Name = "HttpContextObjectFactoryCommandInitializer",
            Usings = new string[] { "System.Linq", "NForza.Cyrus.Abstractions" },
            Initializer = httpContextObjectFactoryInitialization
        };
        var source = liquidEngine.Render(initModel, "CyrusInitializer");
        sourceProductionContext.AddSource($"HttpContextObjectFactoryCommands.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private string AddCommandHandlerMappings(SourceProductionContext sourceProductionContext, IEnumerable<IMethodSymbol> handlers, IEnumerable<IMethodSymbol> validators, Compilation compilation, LiquidEngine liquidEngine)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var handler in handlers.Where(h => h.HasCommandRoute()))
        {
            var validator = validators.FirstOrDefault(v => v.Parameters.Length == 1 && v.Parameters[0].Type.Equals(handler.Parameters[0].Type, SymbolEqualityComparer.Default));
            var command = new
            {
                Path = TypeAnnotations.AugmentRouteWithTypeAnnotations(handler.GetCommandRoute(), handler.Parameters[0].Type),
                Verb = handler.GetCommandVerb(),
                HasBody = handler.HasParametersInBody(),
                CommandType = handler.Parameters[0].Type.ToFullName(),
                CommandName = handler.Parameters[0].Type.Name,
                AdapterMethod = handler.GetAdapterMethodName().ToString(),
                ValidatorMethod = validator,
                ValidatorMethodIsStatic = validator?.IsStatic,
                ValidatorMethodName = validator?.Name,
                ValidatorClass = validator?.ContainingType?.ToFullName(),
                HasReturnType = handler.ReturnType.SpecialType != SpecialType.System_Void && !(handler.ReturnsTask() && handler.TypeArguments.Any()),
                CommandInvocation = handler.GetCommandInvocation(variableName: "cmd"),
                Attributes = handler.GetAttributes().Select(a => a.ToNewInstanceString()).Where(a => a != null)
            };
            sb.AppendLine(liquidEngine.Render(command, "MapCommand"));
        }
        return sb.ToString().Trim();
    }

}