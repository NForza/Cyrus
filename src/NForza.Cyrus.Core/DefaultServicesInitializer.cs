using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Options;
using NForza.Cyrus.Abstractions;
using NForza.Cyrus.Abstractions.Model;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi;

public class DefaultCyrusServices : ICyrusInitializer
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IQueryProcessor, QueryProcessor>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddHttpContextAccessor();
        services.AddSingleton<IHttpContextObjectFactory, HttpContextObjectFactory>();
        services.AddTransient<IConfigureOptions<JsonOptions>, JsonOptionsConfigurator>();

        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options => options.AddSchemaTransformer<CyrusOpenApiSchemaTransformer>());

        services.Scan(scan => scan
            .FromDependencyContext(DependencyContext.Default!)
            .AddClasses(classes => classes.AssignableTo<ICyrusModel>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }
}