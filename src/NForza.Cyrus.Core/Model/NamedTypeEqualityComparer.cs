using NForza.Cyrus.Abstractions.Model;

namespace NForza.Cyrus.Model;

public static class NamedTypeEqualityComparer
{
    public static NamedTypeEqualityComparer<TModel> Instance<TModel>() where TModel : INamedModelType => new NamedTypeEqualityComparer<TModel>();
}

public class NamedTypeEqualityComparer<T> : IEqualityComparer<T>
    where T : INamedModelType
{

    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        return x.Name == y.Name;
    }

    public int GetHashCode(T obj)
    {
        return obj.Name.GetHashCode();
    }
}
