using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cqrs;

public class CqrsOptions(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
