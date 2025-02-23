using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NForza.Cyrus.Cqrs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NForza.Cyrus.Cqrs;

public static class CyrusOptionsExtensions
{
    public static CyrusOptions AddCqrsEndpoints(this CyrusOptions options)
    {
        options.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        options.Services.AddHttpContextAccessor();
        options.Services.AddTransient<IConfigureOptions<JsonOptions>, JsonOptionsConfigurator>();
        return options;
    }

    public static CyrusOptions ConfigureJsonConverters(this CyrusOptions options)
    {
        return options;
    }
}
