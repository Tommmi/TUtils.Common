using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TUtils.Common.Tasks;

public sealed class TaskStorage<T> where T : class
{
    private static readonly AsyncLocal<ConcurrentDictionary<string, object>> _context = new();

    private readonly string _id;
    private string Key => $"{typeof(T).FullName}:{_id}";

    public TaskStorage(string id)
    {
        _id = id ?? throw new ArgumentNullException(nameof(id));
    }

    public T Value
    {
        get
        {
            var dict = _context.Value ??= new ConcurrentDictionary<string, object>();
            if (dict.TryGetValue(Key, out var obj) && obj is T val && val != null)
                return val;

            return null;
        }
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var dict = _context.Value ??= new ConcurrentDictionary<string, object>();
            dict[Key] = value;
        }
    }

    public void Clear()
    {
        var dict = _context.Value;
        if (dict != null)
            dict.TryRemove(Key, out _);
    }
}