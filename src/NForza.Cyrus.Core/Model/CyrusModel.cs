using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

public static class CyrusModel
{
    public static ICyrusModel Aggregate(IServiceProvider serviceProvider)
    {
        var models = serviceProvider.GetServices<ICyrusModel>();
        return new CyrusModelAggregator(models);
    }

    public static string GetAsJson()
    {
        var models = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsAssignableTo(typeof(ICyrusModel)) && !type.IsAbstract)
            .Where(type => type.Name != "CyrusModelAggregator")
            .Select(type => (ICyrusModel)Activator.CreateInstance(type)!);
        return new CyrusModelAggregator(models).AsJson();
    }

}