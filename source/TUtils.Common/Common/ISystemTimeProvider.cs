using System;

namespace TUtils.Common
{
	public interface ISystemTimeProvider
	{
		DateTime LocalTime { get; }
		DateTime UtcTime { get; }
	}
}
