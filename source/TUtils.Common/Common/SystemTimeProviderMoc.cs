using System;

namespace TUtils.Common
{
	public class SystemTimeProviderMoc : ISystemTimeProvider
	{
		private DateTime _initTimeLocal;
		private readonly DateTime _startTime;
		private readonly object _sync = new object();

		public SystemTimeProviderMoc(DateTime initTimeLocal)
		{
			this._initTimeLocal = initTimeLocal;
			_startTime = DateTime.Now;
		}

		public void MakeTimeJump(TimeSpan timeSpan)
		{
			lock (_sync)
			{
				_initTimeLocal = _initTimeLocal.Add(timeSpan);
			}
		}

		public void SetTime(DateTime newTime)
		{
			lock (_sync)
			{
				_initTimeLocal = newTime;
			}
		}


		public DateTime LocalTime
		{
			get
			{
				lock (_sync)
				{
					return _initTimeLocal.Add(DateTime.Now - _startTime);
				}
			}
		}

		public DateTime UtcTime => ((ISystemTimeProvider) this).LocalTime.ToUniversalTime();
	}
}