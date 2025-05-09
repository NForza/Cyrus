﻿using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NForza.Cyrus.Generators.Roslyn;

namespace NForza.Cyrus.Generators.Validators;

public class ValidatorGenerator : CyrusGeneratorBase
{
    public override void GenerateSource(SourceProductionContext spc, CyrusGenerationContext cyrusProvider)
    {
        if (cyrusProvider.Validators.Any())
        {
            var validators = cyrusProvider.Validators;

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            var validatorRegistrations = string.Join(Environment.NewLine,
                    cyrusProvider.Validators
                        .Select(ch => ch.ContainingType)
                        .Where(x => x != null)
                        .Where(x => !x.IsStatic)
                        .Distinct(SymbolEqualityComparer.Default)
                        .Select(cht => $" services.AddTransient<{cht.ToFullName()}>();"));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            if (!string.IsNullOrEmpty(validatorRegistrations))
            {
                var ctx = new
                {
                    Usings = new string[] { "NForza.Cyrus.Cqrs" },
                    Namespace = "Validators",
                    Name = "ValidatorRegistration",
                    Initializer = validatorRegistrations
                };

                var fileContents = cyrusProvider.LiquidEngine.Render(ctx, "CyrusInitializer");
                spc.AddSource("ValidatorRegistration.g.cs", SourceText.From(fileContents, Encoding.UTF8));
            }
        }
    }
}