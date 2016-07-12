using System;

namespace TUtils.Common.Logging.Common
{
	public interface ILoggingValueKey
	{
		/// <summary>
		/// Identifies the logging value type
		/// </summary>
		Guid Guid { get; }
		/// <summary>
		/// value of a concrete cell in the log table. For example the timestamp for the logging key PredefinedLoggingValueIDs.Timestamp
		/// </summary>
		string ElementName { get; }
		bool IsFilterable{ get; }

	}
}