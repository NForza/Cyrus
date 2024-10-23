using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

% Namespaces %

namespace NForza.Cqrs.WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonConverters(this IServiceCollection services)
    {
        % AddJsonConverters %
        return services;
    }
}

