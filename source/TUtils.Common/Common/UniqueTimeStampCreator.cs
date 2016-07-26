using System;

namespace TUtils.Common.Common
{
	public class UniqueTimeStampCreator : IUniqueTimeStampCreator
	{
		private  long _lastTimeStamp = DateTime.UtcNow.Ticks;
		private object _sync = new object();

		long IUniqueTimeStampCreator.Create()
		{
			lock (_sync)
			{
				long timestamp = DateTime.UtcNow.Ticks;
				if (timestamp <= _lastTimeStamp)
					timestamp += _lastTimeStamp+1;
				_lastTimeStamp = timestamp;
				return timestamp;
			}
		}
	}
}