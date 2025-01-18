using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;
using System.Text.Json.Serialization;

% Namespaces %

namespace NForza.Cyrus.Abstractions;

public static class CyrusOptionsJsonConverterExtensions
{
    public static IServiceCollection AddJsonConverters(this IServiceCollection services)
    {
        % AddJsonConverters %
        return services;
    }

    private static Dictionary<Type, Type> allTypes = new() {  % AllTypes % };

    public static CyrusOptions AddTypedIdSerializers(this CyrusOptions options)
    {
        options.Services.AddSingleton(sp => new TypedIdDictionary(allTypes));
        options.Services.AddJsonConverters();
        return options;
    }
}