using System.Threading.Tasks;

namespace NForza.Cyrus.Aggregates;

public interface IAggregateRootPersistence<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : class
    where TAggregateRootId : struct
{
    ValueTask<TAggregateRoot?> Load(TAggregateRootId? id);
    Task Save(TAggregateRoot aggregateRoot);
}
