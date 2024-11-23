using Microsoft.Extensions.DependencyInjection;

namespace NForza.Lumia.Cqrs;

public class CqrsOptions(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
