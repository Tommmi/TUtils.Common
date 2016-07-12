using System;
using System.Collections.Generic;

namespace TUtils.Common.Logging.Common
{
	/// <summary>
	/// Interface zu einem Logging-Service
	/// </summary>
	public interface ILogWriter
	{
		/// <summary>
		/// True, if the log entry should be logged.
		/// Typically an implementation will check the value logValues[PredefinedLoggingValueIDs.Namespace]
		/// and logValues[PredefinedLoggingValueIDs.Severity] for this purpose.
		/// <example><code><![CDATA[
		/// public bool IsActive(Dictionary<Guid, ILogValue> logValues)
		/// {
		/// 	ILogValue logValue;
		/// 
		/// 	if (!logValues.TryGetValue(PredefinedLoggingValueIDs.Severity.Guid, out logValue))
		/// 		return false;
		/// 	var severityText = logValue.Value;
		/// 	var severity = LogSeverityEnum.INFO;
		/// 	if (!Enum.TryParse(severityText, true, out severity))
		/// 		return false;
		/// 	if (!IsSeverityActive(severity)) // check your self
		/// 		return false;
		/// 
		/// 	if (!logValues.TryGetValue(PredefinedLoggingValueIDs.Namespace.Guid, out logValue))
		/// 		return false;
		/// 	var namespaceText = logValue.Value;
		/// 	if (!IsNamespaceActive(namespaceText)) // check your self
		/// 		return false;
		/// 
		/// 	return true;
		/// }
		/// ]]></code>
		/// </example>
		/// </summary>
		/// <param name="logValues">
		/// A map of values, which are about to be logged now.
		/// guid of value category -> concrete value
		/// See class PredefinedLoggingValueIDs for all predefined categories.
		/// Note ! Due to performance issues there are only filterable log values inserted into the 
		/// map: (ILoggingValueKey.IsFiterable is set to true)
		/// - PredefinedLoggingValueIDs.Severity
		/// - PredefinedLoggingValueIDs.Namespace
		/// That means that these value categories could be part of a logging filter.
		/// </param>
		/// <returns></returns>
		bool IsActive(Dictionary<Guid, ILogValue> logValues);

		///  <summary>
		///  Writes log entry into log.
		///  Will be called only if IsActive(..) returns true;
		///  </summary>
		/// <param name="logValues">
		/// A map of values, which are about to be logged now.
		/// guid of value category -> concrete value
		/// See class PredefinedLoggingValueIDs for all predefined categories.
		/// </param>
		void Write2LogFile(Dictionary<Guid, ILogValue> logValues);
	}
}
