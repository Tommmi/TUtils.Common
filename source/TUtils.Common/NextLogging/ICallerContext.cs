using System.Collections.Generic;

namespace TUtils.Common.Logging
{
	public interface ICallerContext : IDictionary<string, string>
	{
		ILoggerFactory LoggerFactory { get; }
		CallerContextDto CreateDto();
	}
}