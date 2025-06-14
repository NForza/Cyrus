using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Abstractions;

public interface ICyrusInitializer
{
    void RegisterServices(IServiceCollection services);
}