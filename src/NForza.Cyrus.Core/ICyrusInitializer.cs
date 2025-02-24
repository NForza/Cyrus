using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus
{
    internal interface ICyrusInitializer
    {
        void Initialize(IServiceCollection services);
    }
}