using Microsoft.Extensions.DependencyInjection;

namespace NForza.Cyrus.Cqrs;

public class QueryProcessor(IServiceScopeFactory serviceScopeFactory) : IQueryProcessor
{
    public IServiceProvider ServiceProvider => serviceScopeFactory.CreateScope().ServiceProvider;
}
