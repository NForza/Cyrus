namespace NForza.Cyrus.Cqrs;

public interface IQueryProcessor
{
    Task<TResult> QueryInternal<TQuery, TResult>(TQuery query, CancellationToken cancellationToken);
    Task<object> QueryInternal(object query, Type queryType, CancellationToken cancellationToken);
}
