using Microsoft.Extensions.DependencyInjection;
using NForza.Cyrus.Cqrs;
using NForza.Cyrus.WebApi.Policies;
using NForza.Cyrus.WebApi;
using NForza.Cyrus;
using Microsoft.Extensions.DependencyModel;
using NForza.Cyrus.Abstractions.Model;

public class DefaultCyrusServices : ICyrusInitializer
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddSingleton<ICqrsFactory, HttpContextCqrsFactory>();
        services.AddSingleton<IQueryProcessor, QueryProcessor>();
        services.AddSingleton<ICommandBus, LocalCommandBus>();
        services.AddSingleton<DefaultCommandInputMappingPolicy>();
        services.AddHttpContextAccessor();

        services.Scan(scan => scan
            .FromDependencyContext(DependencyContext.Default!)
            .AddClasses(classes => classes.AssignableTo<ICyrusModel>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }
}