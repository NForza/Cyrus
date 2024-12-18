namespace NForza.Cyrus.Core;

public class MultiMap<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, List<TValue>> values = [];

    public void Add(TKey key, TValue value)
    {
        if (!values.TryGetValue(key, out var list))
        {
            list = [];
            values[key] = list;
        }
        list.Add(value);
    }

    public IEnumerable<TValue> GetValues(TKey key)
        => values.TryGetValue(key, out var list) ? list : [];

    public bool Remove(TKey key, TValue value)
    {
        if (values.TryGetValue(key, out var list))
        {
            bool removed = list.Remove(value);
            if (list.Count == 0) values.Remove(key);
            return removed;
        }
        return false;
    }
}