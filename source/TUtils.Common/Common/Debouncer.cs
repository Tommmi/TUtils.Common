using System;
using System.Collections.Generic;

namespace TUtils.Common
{
	public class Debouncer : IDebouncer
	{
		private readonly int _debounceTimeMs;
		private readonly object _sync = new object();
		private readonly Dictionary<object,DateTime> _elapseTimes = new Dictionary<object, DateTime>();

		public Debouncer(int debounceTimeMs)
		{
			_debounceTimeMs = debounceTimeMs;
		}

		bool IDebouncer.ShouldIgnore(object inputChannel)
		{
			if ( inputChannel == null)
				throw new ArgumentException("761gdqk32r9rgh");
			lock (_sync)
			{
				DateTime elapseTime;
				var now = DateTime.Now;
				if (_elapseTimes.TryGetValue(inputChannel, out elapseTime))
				{
					if (now < elapseTime)
						return true;
				}

				elapseTime = now.AddMilliseconds(_debounceTimeMs);
				_elapseTimes[inputChannel] = elapseTime;
			}

			return false;
		}
	}
}