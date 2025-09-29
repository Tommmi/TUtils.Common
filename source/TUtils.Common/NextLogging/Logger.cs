using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using TUtils.Common.Logging.Common;

namespace TUtils.Common.Logging
{
	public class Logger : ILogger
	{
		private readonly ITLog _log;
		private readonly ICallerContext _callerContext;
		private readonly object _loggingInstance;
		private static IDictionary<string, Guid> _knownLogGuids = new ConcurrentDictionary<string, Guid>();
		private static object _sync = new object();

		public Logger(
			ITLog log,
			ICallerContext callerContext, 
			object loggingInstance)
		{
			_log = log;
			_callerContext = callerContext;
			_loggingInstance = loggingInstance;
		}

		public void LogInfo(Func<object> map = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			if (_log.IsActive(LogSeverityEnum.INFO, loggingInstance: _loggingInstance))
			{
				_log.LogWithAdditionalValues(LogSeverityEnum.INFO, loggingInstance:_loggingInstance, withStackTrace:false, formattedText:string.Empty,parameters:null,GetLogValues(map,memberName:memberName,lineNumber:lineNumber));
			}
		}

		public void LogWarn(Func<object> map = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			_log.LogWithAdditionalValues(LogSeverityEnum.WARNING, loggingInstance: _loggingInstance, withStackTrace: false, formattedText: string.Empty, parameters: null, GetLogValues(map, memberName: memberName, lineNumber: lineNumber));
		}

		public void LogError(Func<object> map = null, Exception e = null, [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			_log.LogWithAdditionalValues(LogSeverityEnum.WARNING, loggingInstance: _loggingInstance, withStackTrace: false, formattedText: string.Empty, parameters: null, GetLogValues(map, memberName: memberName, lineNumber: lineNumber));
		}

		private ILogValue[] GetLogValues(Func<object> map, string memberName, int lineNumber)
		{
			List<ILogValue> logValues = new List<ILogValue>();

			if (map != null)
			{
				var valueMap = map();
				var type = valueMap.GetType();
				var properties = type.GetProperties();
				lock (_sync)
				{
					foreach (var property in properties)
					{
						var name = property.Name;
						var value = property.GetValue(valueMap)?.ToString();
						if (!_knownLogGuids.TryGetValue(name, out var guid))
						{
							_knownLogGuids[name] = guid = Guid.NewGuid();
						}

						logValues.Add(new LogValue(
							loggingValueId: new LoggingValueKey(
								guid: guid,
								elementName: name,
								isFilterable: false),
							logValue: value));
					}
				}
			}

			var st = new StackTrace(skipFrames: 0, fNeedFileInfo: false);
			var frames = st.GetFrames() ?? [];

			var externalType =
				frames.Select(f => f.GetMethod()?.DeclaringType)
					.FirstOrDefault(t =>
						t != null
						&& t != this.GetType());

			var className = externalType?.FullName ?? "<unknown>";

			logValues.Add(new LogValue(
				loggingValueId: new LoggingValueKey(
					guid: PredefinedLoggingValueIDs.Class.Guid,
					elementName: PredefinedLoggingValueIDs.Class.ElementName,
					isFilterable: true),
				logValue: className));

			logValues.Add(new LogValue(
				loggingValueId: new LoggingValueKey(
					guid: PredefinedLoggingValueIDs.Action.Guid,
					elementName: PredefinedLoggingValueIDs.Action.ElementName,
					isFilterable: true),
				logValue: memberName));

			logValues.Add(new LogValue(
				loggingValueId: new LoggingValueKey(
					guid: PredefinedLoggingValueIDs.Line.Guid,
					elementName: PredefinedLoggingValueIDs.Line.ElementName,
					isFilterable: false),
				logValue: lineNumber.ToString()));


			return logValues.ToArray();
		}
	}
}