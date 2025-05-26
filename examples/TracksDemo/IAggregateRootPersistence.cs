using Microsoft.EntityFrameworkCore;

namespace TracksDemo;

public interface IAggregateRootPersistence<TAggregate, TAggregateId> 
    where TAggregate : class
    where TAggregateId : struct
{
    TAggregate? Get(TAggregateId? id);
    void Save(TAggregate aggregateRoot);
}

public class EFAggregateRootPersistence<TAggregate, TAggregateId, TDbContext>(TDbContext context) : IAggregateRootPersistence<TAggregate, TAggregateId>
    where TAggregate : class
    where TAggregateId : struct
    where TDbContext: DbContext
{
    public TAggregate? Get(TAggregateId? id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return context.Set<TAggregate>().Find(id);
    }

    public void Save(TAggregate aggregateRoot)
    {
        context.SaveChanges();
    }
}