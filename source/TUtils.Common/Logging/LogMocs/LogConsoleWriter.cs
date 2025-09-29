using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TUtils.Common.Common;
using TUtils.Common.Logging.Common;

namespace TUtils.Common.Logging.LogMocs
{
	public class LogConsoleWriter : LogWriterBase, ILogWriter
	{
		private readonly LogSeverityEnum _minSeverity;
		private readonly bool _writeToDebug;
		private readonly List<KeyValuePair<string, bool>> _configurations;

		public LogConsoleWriter(
			LogSeverityEnum minSeverity,
			List<string> namespacesWhiteList,
			List<string> namespacesBlackList,
			bool writeToDebug)
		{
			_minSeverity = minSeverity;
			_writeToDebug = writeToDebug;
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


		bool ILogWriter.IsActive(Dictionary<Guid, ILogValue> logValues)
		{
			string strNamespace;
			LogSeverityEnum severity;
			string severityText;

			if (!TryGetValue(logValues, PredefinedLoggingValueIDs.Severity, out severityText))
				return false;
			if (!Enum.TryParse(severityText, true, out severity))
				return false;

			if (severity > _minSeverity)
				return false;

			if (!TryGetValue(logValues, PredefinedLoggingValueIDs.Namespace, out strNamespace))
				return false;

			return _configurations
				.FirstOrDefault(c => strNamespace.StartsWith(c.Key, StringComparison.InvariantCultureIgnoreCase))
				.Value;
		}

		void ILogWriter.Write2LogFile(Dictionary<Guid, ILogValue> logValues)
		{
			if(((ILogWriter)this).IsActive(logValues))
			{
				var text = GetLogTextInExcelStyle(logValues);

				if (_writeToDebug)
				{
					Debug.WriteLine(text);
				}
				else
				{
					Console.WriteLine(text);
				}
			}
		}
	}
}
