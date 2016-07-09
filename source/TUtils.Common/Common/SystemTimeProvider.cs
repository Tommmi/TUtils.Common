using System;

namespace TUtils.Common.Common
{
	public class SystemTimeProvider : ISystemTimeProvider
	{
		DateTime ISystemTimeProvider.LocalTime => DateTime.Now;

		DateTime ISystemTimeProvider.UtcTime => DateTime.UtcNow;
	}
}