using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TUtils.Common.Logging.Common;

namespace TUtils.Common.Logging.LogMocs
{
	public class LogConsole : ILogXImplementor
	{
		private readonly LogSeverityEnum _minSeverity;
		private readonly List<KeyValuePair<string, bool>> _configurations;

		public LogConsole(
			LogSeverityEnum minSeverity,
			List<string> namespacesWhiteList,
			List<string> namespacesBlackList)
		{
			_minSeverity = minSeverity;
			_configurations = namespacesWhiteList
				.Select(k => new KeyValuePair<string, bool>(k == "*" ? "" : k, true))
				.Concat(namespacesBlackList.Select(k => new KeyValuePair<string, bool>(k == "*" ? "" : k, false)))
				.OrderByDescending(item => item.Key.Length)
				.ToList();
		}

		private bool TryGetValue(Dictionary<Guid, ILogValue> logValues, ILoggingValueKey key, out string value)
		{
			ILogValue logValue;
			if (logValues.TryGetValue(key.Guid, out logValue))
			{
				value = logValue.Value;
				return true;
			}
			value = string.Empty;
			return false;
		}


		bool ILogXImplementor.IsActive(Dictionary<Guid, ILogValue> logValues)
		{
			string strNamespace;
			LogSeverityEnum severity;
			string severityText;

			if (!TryGetValue(logValues, PredefinedLoggingValueIDs.Severity, out severityText))
				return false;
			if (!Enum.TryParse(severityText, true, out severity))
				return false;

			if (severity >= _minSeverity)
				return false;

			if (!TryGetValue(logValues, PredefinedLoggingValueIDs.Namespace, out strNamespace))
				return false;

			return _configurations
				.FirstOrDefault(c => strNamespace.StartsWith(c.Key, StringComparison.InvariantCultureIgnoreCase))
				.Value;
		}

		void ILogXImplementor.Write2LogFile(Dictionary<Guid, ILogValue> logValues)
		{
			string strNamespace;
			TryGetValue(logValues, PredefinedLoggingValueIDs.Namespace, out strNamespace);
			string loggingText;
			if (!TryGetValue(logValues, PredefinedLoggingValueIDs.ExceptionObject, out loggingText))
			{
				TryGetValue(logValues, PredefinedLoggingValueIDs.LoggingText, out loggingText);
			}

			string severity;
			TryGetValue(logValues, PredefinedLoggingValueIDs.Severity, out severity);
			if (severity == LogSeverityEnum.INFO.ToString())
				severity = string.Empty;
			else
				severity += " - ";

			Debug.WriteLine(
				$"{severity}{strNamespace}: {loggingText}");
		}
	}
}
