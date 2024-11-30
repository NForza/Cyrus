using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class CqrsOptions(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
