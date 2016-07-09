using System;

namespace TUtils.Common.Logging.Common
{
	public interface ILoggingValueKey
	{
		Guid Guid { get; }
		string ElementName { get; }
		bool IsFilterable{ get; }

	}
}