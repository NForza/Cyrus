using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus;

public interface ICyrusInitializer
{
    void RegisterServices(IServiceCollection services);
}