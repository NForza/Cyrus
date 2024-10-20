using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace NForza.TypedIds;

public static class TypedIdHelper
{
    private static readonly ConcurrentDictionary<Type, Delegate> StronglyTypedIdFactories = new();

    public static Func<TValue, object> GetFactory<TValue>(Type stronglyTypedIdType)
        where TValue : notnull
    {
        return (Func<TValue, object>)StronglyTypedIdFactories.GetOrAdd(
            stronglyTypedIdType,
            CreateFactory<TValue>);
    }

    private static Func<TValue, object> CreateFactory<TValue>(Type stronglyTypedIdType)
        where TValue : notnull
    {
        if (!IsStronglyTypedId(stronglyTypedIdType))
            throw new ArgumentException($"Type '{stronglyTypedIdType}' is not a strongly-typed id type", nameof(stronglyTypedIdType));

        var ctor = stronglyTypedIdType.GetConstructor([typeof(TValue)]) ?? throw new ArgumentException($"Type '{stronglyTypedIdType}' doesn't have a constructor with one parameter of type '{typeof(TValue)}'", nameof(stronglyTypedIdType));
        var param = Expression.Parameter(typeof(TValue), "value");
        var body = Expression.New(ctor, param);
        var lambda = Expression.Lambda<Func<TValue, object>>(body, param);
        return lambda.Compile();
    }

    public static bool IsStronglyTypedId(Type type) => IsStronglyTypedId(type, out _);

    public static bool IsStronglyTypedId(Type type, [NotNullWhen(true)] out Type? idType)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.BaseType is Type baseType &&
            baseType.IsGenericType &&
            baseType.GetGenericTypeDefinition() == typeof(TypedId<>))
        {
            idType = baseType.GetGenericArguments()[0];
            return true;
        }

        idType = null;
        return false;
    }
}
