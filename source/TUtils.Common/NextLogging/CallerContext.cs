using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TUtils.Common.Logging
{
	public class CallerContext : ICallerContext
	{
		private IDictionary<string, string> _map = new ConcurrentDictionary<string, string>();

		public CallerContext(ILoggerFactory loggerFactory)
		{
			LoggerFactory = loggerFactory;
			_map["CorrelationId"] = Guid.NewGuid().ToString();
		}

		public CallerContext(ILoggerFactory loggerFactory, CallerContextDto callerContextDto) : this(loggerFactory)
		{
			if (callerContextDto != null)
			{
				foreach (var keyValue in callerContextDto.KeyValues)
				{
					_map[keyValue.Key] = keyValue.Value;
				}
			}
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return _map.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _map.GetEnumerator();
		}

		public void Add(KeyValuePair<string, string> item)
		{
			_map.Add(item);
		}

		public void Clear()
		{
			_map.Clear();
		}

		public bool Contains(KeyValuePair<string, string> item)
		{
			return _map.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			_map.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, string> item)
		{
			return _map.Remove(item);
		}

		public int Count => _map.Count;

		public bool IsReadOnly => _map.IsReadOnly;

		public void Add(string key, string value)
		{
			_map.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _map.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return _map.Remove(key);
		}

		public bool TryGetValue(string key, out string value)
		{
			return _map.TryGetValue(key, out value);
		}

		public string this[string key]
		{
			get => _map[key];
			set => _map[key] = value;
		}

		public ICollection<string> Keys => _map.Keys;

		public ICollection<string> Values => _map.Values;
		public ILoggerFactory LoggerFactory { get; }

		public CallerContextDto CreateDto()
		{
			return new CallerContextDto(_map.Select(x=>new CallerContextKeyValuePair(key: x.Key, value:x.Value)));
		}
	}
}