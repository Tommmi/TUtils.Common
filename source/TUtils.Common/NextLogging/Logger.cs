using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TUtils.Common.Logging.Common;

namespace TUtils.Common.Logging
{
	public class Logger : ILogger
	{
		private readonly ITLog _log;
		private readonly ICallerContext _callerContext;
		private readonly object _loggingInstance;
		private IDictionary<string, Guid> _knownLogGuids = new Dictionary<string, Guid>();
		private object _sync = new object();

		public Logger(
			ITLog log,
			ICallerContext callerContext, 
			object loggingInstance)
		{
			_log = log;
			_callerContext = callerContext;
			_loggingInstance = loggingInstance;
		}

		public void LogInfo(Func<object> map = null)
		{
			if (_log.IsActive(LogSeverityEnum.INFO, loggingInstance: _loggingInstance))
			{
				_log.LogWithAdditionalValues(LogSeverityEnum.INFO, loggingInstance:_loggingInstance, withStackTrace:false, formattedText:string.Empty,parameters:null,GetLogValues(map));
			}
		}

		public void LogWarn(Func<object> map = null)
		{
			_log.LogWithAdditionalValues(LogSeverityEnum.WARNING, loggingInstance: _loggingInstance, withStackTrace: false, formattedText: string.Empty, parameters: null, GetLogValues(map));
		}

		public void LogError(Func<object> map = null, Exception e = null)
		{
			_log.LogWithAdditionalValues(LogSeverityEnum.WARNING, loggingInstance: _loggingInstance, withStackTrace: false, formattedText: string.Empty, parameters: null, GetLogValues(map));
		}

		private ILogValue[] GetLogValues(Func<object> map)
		{
			var valueMap = map();
			var type = valueMap.GetType();
			var properties = type.GetProperties();
			List<ILogValue> logValues = new List<ILogValue>();
			lock (_sync)
			{
				foreach (var property in properties)
				{
					var name = property.Name;
					var value = property.GetValue(valueMap).ToString();
					if (!_knownLogGuids.TryGetValue(name, out var guid))
					{
						_knownLogGuids[name] = guid = Guid.NewGuid();
					}

					logValues.Add( new LogValue(
						loggingValueId: new LoggingValueKey(
							guid: guid,
							elementName: name,
							isFilterable: false),
						logValue: value));
				}
			}

			return logValues.ToArray();
		}
	}
}