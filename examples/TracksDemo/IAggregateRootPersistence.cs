namespace TracksDemo;

public interface IAggregateRootPersistence<TAggregate, TAggregateId> 
    where TAggregate : class
{
    TAggregate Get(TAggregateId? id);
    void Save(TAggregate aggregateRoot);
}
