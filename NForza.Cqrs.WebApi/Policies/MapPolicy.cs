
namespace NForza.Cqrs.WebApi.Policies;

public class MapPolicy<T>(Func<T, object?> mapFunction) : QueryResultPolicy
    where T : class?
{
    public override object? MapFromQueryResult(object? result)
    {
        return result == null ? null : mapFunction((T)result);
    }
}