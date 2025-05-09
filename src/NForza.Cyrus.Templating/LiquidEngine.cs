﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Fluid;

namespace NForza.Cyrus.Templating;

public class LiquidEngine
{
    TemplateFileProvider fileProvider;
    private TemplateOptions options;

    public LiquidEngine(Assembly assembly, Dictionary<string, string> templateOverrides, Action<TemplateOptions> configure = null)
    {
        fileProvider = new TemplateFileProvider(assembly, templateOverrides);
        options = new TemplateOptions();
        options.Filters.AddFilter("as-contract", CyrusFilters.ContractName);
        options.Filters.AddFilter("generated-hub-name", CyrusFilters.GeneratedHubName);
        options.Filters.AddFilter("camel-cased", CyrusFilters.CamelCased);
        options.FileProvider = fileProvider;
        options.MemberAccessStrategy = UnsafeMemberAccessStrategy.Instance;
        configure?.Invoke(options);
    }

    private TemplateContext GetContext(object model)
    {
        return new TemplateContext(model, options);
    }

    private IFluidTemplate GetTemplate(string path)
    {
        var parser = new FluidParser();
        var template = parser.Parse(fileProvider.GetTemplateContents(path));
        return template;
    }

    public string Render(object model, string templateName)
    {
        try
        {
            IFluidTemplate template = GetTemplate(templateName);
            var result = template.Render(GetContext(model));
            return result;

        }
        catch (ParseException e)
        {
            throw new InvalidOperationException($"Can't load template \"{templateName}\": {e.Message}", e);
        }
    }
}