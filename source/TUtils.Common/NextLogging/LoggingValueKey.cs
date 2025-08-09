using System;
using TUtils.Common.Logging.Common;

namespace TUtils.Common.Logging
{
	public class LoggingValueKey : ILoggingValueKey
	{
		public LoggingValueKey(Guid guid, string elementName, bool isFilterable)
		{
			Guid = guid;
			ElementName = elementName;
			IsFilterable = isFilterable;
		}

		public Guid Guid { get; }
		public string ElementName { get; }
		public bool IsFilterable { get; }
	}
}