using System;
using TUtils.Common.Logging.Common;

namespace TUtils.Common.Logging
{
	public interface ITLog
	{
		void LogInfo(
			object loggingInstance,
			string formattedText,
			params Func<object>[] parameters);
		
		void Log(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			string formattedText,
			params Func<object>[] parameters);

		void LogWithAdditionalValues(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			bool withStackTrace,
			string formattedText,
			Func<object>[] parameters,
			params ILogValue[] additionalLogValues);

		void LogWithAdditionalValues(
			LogSeverityEnum errorLevel,
			string strNamespace,
			bool withStackTrace,
			string formattedText,
			Func<object>[] parameters,
			params ILogValue[] additionalLogValues);


		void LogWithStackTrace(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			string formattedText,
			params Func<object>[] parameters);

		void LogInfo(
			string strNamespace,
			string formattedText,
			params Func<object>[] parameters);

		void Log(
			LogSeverityEnum errorLevel,
			string strNamespace,
			string formattedText,
			params Func<object>[] parameters);

		void LogWithStackTrace(
			LogSeverityEnum errorLevel,
			string strNamespace,
			string formattedText,
			params Func<object>[] parameters);

		void LogException(Exception exception,params ILogValue[] additionalLogValues);

		bool IsActive(
			LogSeverityEnum errorLevel,
			string strNamespace,
			params ILogValue[] additionalLogValues);
		
		bool IsActive(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			params ILogValue[] additionalLogValues);
	}
}