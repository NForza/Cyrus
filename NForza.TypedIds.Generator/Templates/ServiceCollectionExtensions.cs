using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

% Namespaces %

namespace NForza.TypedIds;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonConverters(this IServiceCollection services)
    {
        % AddJsonConverters %
        return services;
    }

    private static Dictionary<Type, Type> allTypes = new() {  % AllTypes % };

    public static IServiceCollection AddTypedIds(this IServiceCollection services)
    {
        services.AddSingleton(sp => new TypedIdDictionary(allTypes));
        services.AddJsonConverters();
        return services;
    }
}

