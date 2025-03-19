using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Config;
using NForza.Cyrus.Generators.Roslyn;
using NForza.Cyrus.Templating;

namespace NForza.Cyrus.Generators.WebApi;

public static class WebApiContractGenerator
{
    public static void GenerateContracts(IEnumerable<INamedTypeSymbol> contracts, SourceProductionContext sourceProductionContext, LiquidEngine liquidEngine)
    {
        foreach (var contract in contracts)
        {
            if (contract.IsRecordType())
            {
                var constructorArguments = contract.GetConstructorArguments();
                var model = new
                {
                    Namespace = contract.ContainingNamespace,
                    Name = contract.Name,
                    FullName = contract.ToFullName(),
                    ConstructorArguments = constructorArguments.Select(p =>
                        new
                        {
                            Name = p.Name,
                            Type = p.Type.ToFullName(),
                            IsNullable = p.Type.IsNullable()
                        }).ToList(),
                    Properties = contract.GetPublicProperties().Select(p =>
                        new
                        {
                            Name = p.Name,
                            Type = p.Type.ToFullName(),
                            IsNullable = p.Type.IsNullable()
                        }).ToList()
                };

                var fileContents = liquidEngine.Render(model, "WebApiContractRecord");

                sourceProductionContext.AddSource(
                   $"{contract.Name}Contract.g.cs",
                   SourceText.From(fileContents, Encoding.UTF8));
            }
        }
    }
}