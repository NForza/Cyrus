using Microsoft.EntityFrameworkCore;
using NForza.Cyrus.Aggregates;

namespace NForza.Cyrus.EntityFramework;

public class EntityFrameworkPersistence<TAggregateRoot, TAggregateRootId, TDbContext>(TDbContext ctx) : IAggregateRootPersistence<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : class
    where TAggregateRootId : struct
    where TDbContext : DbContext
{
    public ValueTask<TAggregateRoot?> Load(TAggregateRootId? id)
        => ctx.Set<TAggregateRoot>().FindAsync(id);

    public Task Save(TAggregateRoot aggregateRoot)
        => ctx.SaveChangesAsync();
}
