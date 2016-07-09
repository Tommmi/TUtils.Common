using System;

namespace TUtils.Common.Logging.Common
{
	public class LogValue : ILogValue
	{
		public ILoggingValueKey Key { get; }

		private string _logValue;
		public string Value => _logValue ?? (_logValue = _getLogText());

		private readonly Func<string> _getLogText;

		private LogValue(ILoggingValueKey loggingValueKey)
		{
			Key = loggingValueKey;
		}

		public LogValue(ILoggingValueKey loggingValueId, string logValue)
			: this(loggingValueKey: loggingValueId)
		{
			_logValue = logValue;
		}
		public LogValue(ILoggingValueKey loggingValueId, Func<string> getLogValue)
			: this(loggingValueKey: loggingValueId)
		{
			_getLogText = getLogValue;
		}

	}
}