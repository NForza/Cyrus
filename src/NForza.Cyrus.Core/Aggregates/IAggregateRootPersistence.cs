namespace NForza.Cyrus.Aggregates;

public interface IAggregateRootPersistence<TAggregate, TAggregateId> 
    where TAggregate : class
    where TAggregateId : struct
{
    TAggregate? Get(TAggregateId? id);
    void Save(TAggregate aggregateRoot);
}
