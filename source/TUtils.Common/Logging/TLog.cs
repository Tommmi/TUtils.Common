using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using TUtils.Common.Extensions;
using TUtils.Common.Logging.Common;

// ReSharper disable MemberCanBePrivate.Global

namespace TUtils.Common.Logging
{
	/// <summary>
	/// TLog is the recommended implementation of ITLog, but is independent of the 
	/// underlying logging technology. With TLog you may use log4Net as well as any other technology to
	/// write the concrete logging statements.
	/// TLog needs to be initialized with a concrete log writer implementing the interface ILogWriter.
	/// There are two suggested implementations of ILogWriter shipped with this library: Log4NetWriter and 
	/// LogMoc. Usage:
	/// <example>
	/// <code><![CDATA[
	/// ITLog logger = new TLog(new Log4NetWriter(),isLoggingOfMethodNameActivated:false);
	/// logger.LogInfo(this,"myComplexObject={0} !",()=>myComplexObject.ToString());
	/// ]]></code></example>
	/// </summary>
	/// <remarks>
	/// TLog enhance the logger with the feature of thread-attached logging values.
	/// Call SetThreadAttachedLogValue() to attach a value to the thread.
	/// The value will be written to each log entry automatically (if the writer doesn't prevent this).
	/// You may also register a value provider:
	/// (see RegisterLogValueProvider())
	/// </remarks>
	// ReSharper disable once InconsistentNaming
	// ReSharper disable once UnusedMember.Global
	public class TLog : ITLog
	{
		#region private

		#region private types
		private class StackTraceFrame
		{
			/// <summary>
			/// may be null
			/// </summary>
			public int? LineNumber { get; }
			/// <summary>
			/// may be null
			/// </summary>
			public string FileName
			{
				get; }
			public string MethodName
			{
				get; }

			public StackTraceFrame(
				string fileName,
				string methodName,
				int? lineNumber)
			{
				FileName = fileName;
				MethodName = methodName;
				LineNumber = lineNumber;
			}
		}
		#endregion

		#region fields

		private readonly object _syncObj = new object();

		/// <summary>
		/// 
		/// </summary>
		private readonly List<ILogValueProvider> _logValueProviders = new List<ILogValueProvider>();

		#endregion

		private const string ThreadlocalStorageData = "TLog.THREADLOCAL_STORAGE_DATA";
		private IEnumerable<ILogValueProvider> LogValueProviders
		{
			get
			{
				lock (_syncObj)
				{
					return _logValueProviders.ToList();
				}
			}
		}

		private Dictionary<Guid, ILogValue> GetAllLogValues(
			object loggingInstance,
			string strNamespace,
			LogSeverityEnum errorLevel,
			Exception e,
			bool withStackTrace,
			bool justFilterableLogValues,
			IEnumerable<ILogValue> additionalLogValues,
			string formattedText,
			params Func<object>[] formattedTextParams
			)
		{
			var logValues = new Dictionary<Guid, ILogValue>();

			// add default log values

			IEnumerable<ILogValue> defaultValues;

			if (justFilterableLogValues)
			{
				defaultValues = GetDefaultFilterableLogValues(
					strNamespace: strNamespace,
					errorLevel: errorLevel,
					loggingInstance: loggingInstance);
			}
			else
			{
				defaultValues = GetDefaultLogValues(
					loggingInstance: loggingInstance,
					strNamespace: strNamespace,
					errorLevel: errorLevel,
					e: e,
					withStackTrace: withStackTrace,
					formattedText: formattedText,
					formattedTextParams: formattedTextParams);
			}

			foreach (var defaultValue in defaultValues)
				logValues[defaultValue.Key.Guid] = defaultValue;

			// add log values from log providers
			var logValueProviders = LogValueProviders;
			foreach (var logValueProvider in logValueProviders)
			{
				foreach (var logValue in logValueProvider.GetLogValues(
					loggingInstance: loggingInstance,
					strNamespace: strNamespace))
				{
					if (!justFilterableLogValues || logValue.Key.IsFilterable)
						logValues[logValue.Key.Guid] = logValue;
				}
			}

			// add log values from thread local storage
			var threadAttachedLogValues = TThreadStorage<Dictionary<Guid, ILogValue>>.GetData(ThreadlocalStorageData);
			foreach (var threadAttachedLogValue in threadAttachedLogValues)
			{
				var logValue = threadAttachedLogValue.Value;
				if (!justFilterableLogValues || logValue.Key.IsFilterable)
					logValues[logValue.Key.Guid] = logValue;
			}

			// add log values from parameter additionalLogValues
			if (additionalLogValues != null)
			{
				foreach (var additionalLogValue in additionalLogValues)
				{
					if (!justFilterableLogValues || additionalLogValue.Key.IsFilterable)
						logValues[additionalLogValue.Key.Guid] = additionalLogValue;
				}
			}


			return logValues;
		}

		private static StackTraceFrame AnalizeStackTraceLine(string stackTraceLine)
		{
			var atTag = "   at ";
			var inTag = " in ";
			var lineTag = ":line ";
			if (!stackTraceLine.StartsWith(atTag))
				return null;
			string fullMethodName;
			string fileName = null;
			int? line = null;
			int idxOfIn = stackTraceLine.IndexOf(inTag, StringComparison.Ordinal);
			if (idxOfIn > 0)
			{
				fullMethodName = stackTraceLine.Substring(atTag.Length, idxOfIn - atTag.Length);
				string rightPart = stackTraceLine.Substring(idxOfIn + inTag.Length);

				int idxOfColon = rightPart.IndexOf(lineTag, StringComparison.Ordinal);
				if (idxOfColon > 0)
				{
					var filePath = rightPart.Left(idxOfColon);
					fileName = Path.GetFileName(filePath);
					int lineNo;
					string txtLine = rightPart.Substring(idxOfColon + lineTag.Length);
					if (int.TryParse(txtLine, out lineNo))
						line = lineNo;
				}
				else
				{
					var filePath = rightPart;
					fileName = Path.GetFileName(filePath);
				}
			}
			else
			{
				fullMethodName = stackTraceLine.Substring(atTag.Length);
			}

			int lastIdxOfPoint = fullMethodName.LastIndexOf('.');
			if (lastIdxOfPoint <= 0)
				return null;

			var methodName = fullMethodName.Substring(lastIdxOfPoint + 1);
			int idxOfBracket = methodName.IndexOf('(');
			if (idxOfBracket > 0)
				methodName = methodName.Left(idxOfBracket);

			return new StackTraceFrame(
				fileName: fileName,
				methodName: methodName,
				lineNumber: line);
		}

		private StackTraceFrame GetStackTraceFrame()
		{
			var stackTrace = Environment.StackTrace;
			var stackTraceLines = stackTrace.Split('\n');
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var stackTraceLine in stackTraceLines)
			{
				var frame = AnalizeStackTraceLine(stackTraceLine);
				var fileName = frame?.FileName;
				if (fileName != null && !fileName.Contains("Log"))
					return frame;
			}
			return null;
		}

		/// <summary>
		/// add "namespace" and "severity"
		/// </summary>
		/// <param name="loggingInstance"></param>
		/// <param name="strNamespace"></param>
		/// <param name="errorLevel"></param>
		/// <returns></returns>
		private IEnumerable<ILogValue> GetDefaultFilterableLogValues(
			object loggingInstance,
			string strNamespace,
			LogSeverityEnum errorLevel
			)
		{
			var logValues = new List<ILogValue>();

			var errorLevelTxt = errorLevel.ToString();

			//Severity
			logValues.Add(new LogValue(
				PredefinedLoggingValueIDs.Severity,
				errorLevelTxt));

			// Namespace
			if (loggingInstance != null)
			{
				logValues.Add(new LogValue(
					PredefinedLoggingValueIDs.Namespace,
					() =>
					{
						var fullNamespace = loggingInstance.GetType().FullName;
						fullNamespace = fullNamespace.Replace('+', '.');
						// remove "'1[[*]]"
						int idxOpen = fullNamespace.IndexOf('`');
						while (idxOpen > 0)
						{
							int idxClose = fullNamespace.IndexOf("]]", StringComparison.Ordinal);
							if (idxClose > 0)
								fullNamespace = fullNamespace.Remove(idxOpen, idxClose - idxOpen + 2);
							idxOpen = fullNamespace.IndexOf('`');
						}
						return fullNamespace;
					}));

			}
			else if (strNamespace != null)
			{
				logValues.Add(new LogValue(
					PredefinedLoggingValueIDs.Namespace,
					strNamespace));
			}

			return logValues;
		}

		/// <summary>
		/// add "namespace", "sebverity", "Timestamp",
		/// "ExceptionObject","LoggingInstanceID","LoggingText",
		/// "StackTrace","ThreadId","ThreadName",
		/// "MethodName", "Filename", "LineNumber",
		/// </summary>
		/// <param name="loggingInstance"></param>
		/// <param name="strNamespace"></param>
		/// <param name="errorLevel"></param>
		/// <param name="e"></param>
		/// <param name="withStackTrace"></param>
		/// <param name="formattedText"></param>
		/// <param name="formattedTextParams"></param>
		/// <returns></returns>
		private IEnumerable<ILogValue> GetDefaultLogValues(
			object loggingInstance,
			string strNamespace,
			LogSeverityEnum errorLevel,
			Exception e,
			bool withStackTrace,
			string formattedText,
			params Func<object>[] formattedTextParams
			)
		{
			var filterableLogValues = GetDefaultFilterableLogValues(
				loggingInstance: loggingInstance,
				strNamespace: strNamespace,
				errorLevel: errorLevel);

			var logValues = new List<ILogValue>();

			logValues.AddRange(filterableLogValues);

			var now = DateTime.Now;
			//Timestamp
			logValues.Add(new LogValue(
				PredefinedLoggingValueIDs.Timestamp,
				now.ToString("HHmmss") + now.Millisecond.ToString("D3")));
			//ExceptionObject
			if (e != null)
				logValues.Add(new LogValue(
					PredefinedLoggingValueIDs.ExceptionObject,
					ExceptionHelper.DumpException(e)));
			// LoggingInstanceID
			if (loggingInstance != null)
				logValues.Add(new LogValue(
					PredefinedLoggingValueIDs.LoggingInstanceId,
					loggingInstance.GetHashCode().ToString(CultureInfo.InvariantCulture)));
			// LoggingText
			if (formattedText != null)
			{
				logValues.Add(new LogValue(
					PredefinedLoggingValueIDs.LoggingText,
					() => formattedTextParams == null || formattedTextParams.Length == 0 ? formattedText : string.Format(
								formattedText,
								formattedTextParams
									.AsEnumerable()
									.Select(p => p())
									.ToArray())));

			}
			else if (e != null)
			{
				logValues.Add(new LogValue(
					PredefinedLoggingValueIDs.LoggingText,
					ExceptionHelper.DumpException(e)));
			}


			// StackTrace
			if (withStackTrace)
				logValues.Add(new LogValue(
					PredefinedLoggingValueIDs.StackTrace,
					() => Environment.StackTrace));
			// ThreadId
			logValues.Add(new LogValue(
				PredefinedLoggingValueIDs.ThreadId,
				Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture)));
			// ThreadName
			logValues.Add(new LogValue(
				PredefinedLoggingValueIDs.ThreadName,
				() => Thread.CurrentThread.Name ?? "NOT SPECIFIED"));
			// MethodName, Filename, LineNumber
			if (IsLoggingOfMethodNameActivated)
			{
				var stackTraceFrame = GetStackTraceFrame();
				if (stackTraceFrame != null)
				{
					logValues.Add(new LogValue(
									  PredefinedLoggingValueIDs.MethodName,
									  stackTraceFrame.MethodName));
					if (stackTraceFrame.FileName != null)
						logValues.Add(new LogValue(
										  PredefinedLoggingValueIDs.Filename,
										  stackTraceFrame.FileName));
					if (stackTraceFrame.LineNumber.HasValue)
						logValues.Add(new LogValue(
										  PredefinedLoggingValueIDs.Line,
										  stackTraceFrame.LineNumber.Value.ToString()));
				}
			}

			return logValues;
		}

		#endregion

		#region public

		/// <summary>
		/// true, if method name, filename and line number should be logged
		/// </summary>
		public bool IsLoggingOfMethodNameActivated { get; }

		public ILogWriter LoggWriter { get; }

		// ReSharper disable once UnusedMember.Global
		public void RegisterLogValueProvider(ILogValueProvider provider)
		{
			lock (_syncObj)
			{
				if (!_logValueProviders.Contains(provider))
					_logValueProviders.Add(provider);
			}
		}

		public TLog(
			ILogWriter logWriter,
			bool isLoggingOfMethodNameActivated)
		{
			LoggWriter = logWriter;
			IsLoggingOfMethodNameActivated = isLoggingOfMethodNameActivated;
		}

		public void LogInfo(
			object loggingInstance,
			string formattedText,
			params Func<object>[] parameters)
		{
			Log(errorLevel: LogSeverityEnum.INFO,
				loggingInstance: loggingInstance,
				formattedText: formattedText,
				parameters: parameters);
		}

		public void Log(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			string formattedText,
			params Func<object>[] parameters)
		{
			Write(
				loggingInstance: loggingInstance,
				strNamespace: null,
				errorLevel: errorLevel,
				e: null,
				withStackTrace: false,
				additionalLogValues: null,
				formattedText: formattedText,
				formattedTextParams: parameters);
		}

		public void LogWithStackTrace(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			string formattedText,
			params Func<object>[] parameters)
		{
			Write(
				errorLevel: errorLevel,
				loggingInstance: loggingInstance,
				withStackTrace: true,
				formattedText: formattedText,
				strNamespace: null,
				e: null,
				additionalLogValues: null,
				formattedTextParams: parameters);
		}

		public void LogInfo(
			string strNamespace,
			string formattedText,
			params Func<object>[] parameters)
		{
			Log(errorLevel: LogSeverityEnum.INFO,
				strNamespace: strNamespace,
				formattedText: formattedText,
				parameters: parameters);
		}

		public void Log(
			LogSeverityEnum errorLevel,
			string strNamespace,
			string formattedText,
			params Func<object>[] parameters)
		{
			Write(
				errorLevel: errorLevel,
				strNamespace: strNamespace,
				withStackTrace: false,
				formattedText: formattedText,
				loggingInstance: null,
				e: null,
				additionalLogValues: null,
				formattedTextParams: parameters);
		}

		public void LogWithStackTrace(
			LogSeverityEnum errorLevel,
			string strNamespace,
			string formattedText,
			params Func<object>[] parameters)
		{
			Write(
				loggingInstance: null,
				strNamespace: strNamespace,
				errorLevel: errorLevel,
				e: null,
				withStackTrace: true,
				additionalLogValues: null,
				formattedText: formattedText,
				formattedTextParams: parameters);
		}

		public void LogException(Exception exception, params ILogValue[] additionalLogValues)
		{
			var strNamespace = exception.GetType().FullName;
			if (!IsActive(LogSeverityEnum.ERROR, strNamespace))
				return;
			Write(
				loggingInstance: null,
				strNamespace: strNamespace,
				errorLevel: LogSeverityEnum.ERROR,
				e: exception,
				withStackTrace: true,
				additionalLogValues: additionalLogValues,
				formattedText: exception.Message);
		}

		public bool IsActive(
			LogSeverityEnum errorLevel,
			string strNamespace,
			params ILogValue[] additionalLogValues)
		{
			return IsActive(
				loggingInstance: null,
				strNamespace: strNamespace,
				errorLevel: errorLevel,
				additionalLogValues: additionalLogValues);
		}

		public bool IsActive(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			params ILogValue[] additionalLogValues)
		{
			return IsActive(
				loggingInstance: loggingInstance,
				strNamespace: null,
				errorLevel: errorLevel,
				additionalLogValues: additionalLogValues);
		}

		public bool IsActive(
			object loggingInstance,
			string strNamespace,
			LogSeverityEnum errorLevel,
			IEnumerable<ILogValue> additionalLogValues)
		{
			var logValues = GetAllLogValues(
				loggingInstance: loggingInstance,
				strNamespace: strNamespace,
				errorLevel: errorLevel,
				e: null,
				withStackTrace: false,
				justFilterableLogValues: true,
				additionalLogValues: additionalLogValues,
				formattedText: null,
				formattedTextParams: null);


			return LoggWriter.IsActive(logValues);
		}
		
		// ReSharper disable once UnusedMember.Global
		public ILogValue GetThreadAttachedLogValue(Guid logValueKey)
		{
			var threadAttachedLogValues = TThreadStorage<Dictionary<Guid, ILogValue>>.GetData(ThreadlocalStorageData);
			ILogValue logValue;
			if (threadAttachedLogValues.TryGetValue(logValueKey, out logValue))
				return logValue;
			return null;
		}

		// ReSharper disable once UnusedMember.Global
		public void SetThreadAttachedLogValue(ILogValue logValue)
		{
			var threadAttachedLogValues = TThreadStorage<Dictionary<Guid, ILogValue>>.GetData(ThreadlocalStorageData);
			threadAttachedLogValues[logValue.Key.Guid] = logValue;
		}

		public void LogWithAdditionalValues(
			LogSeverityEnum errorLevel,
			object loggingInstance,
			bool withStackTrace,
			string formattedText,
			Func<object>[] parameters,
			params ILogValue[] additionalLogValues)
		{
			Write(
				loggingInstance: loggingInstance,
				strNamespace: null,
				errorLevel: errorLevel,
				e: null,
				withStackTrace: withStackTrace,
				additionalLogValues: additionalLogValues,
				formattedText: formattedText,
				formattedTextParams: parameters);
		}


		public void LogWithAdditionalValues(
			LogSeverityEnum errorLevel,
			string strNamespace,
			bool withStackTrace,
			string formattedText,
			Func<object>[] parameters,
			params ILogValue[] additionalLogValues)
		{
			Write(
				loggingInstance: null,
				errorLevel: errorLevel,
				strNamespace: strNamespace,
				e: null,
				withStackTrace: withStackTrace,
				additionalLogValues: additionalLogValues,
				formattedText: formattedText,
				formattedTextParams: parameters);
		}

		#endregion

		#region protected

		protected virtual void Write(
			object loggingInstance,
			string strNamespace,
			LogSeverityEnum errorLevel,
			Exception e,
			bool withStackTrace,
			IEnumerable<ILogValue> additionalLogValues,
			string formattedText,
			params Func<object>[] formattedTextParams
			)
		{
			var logValues = GetAllLogValues(
				loggingInstance: loggingInstance,
				strNamespace: strNamespace,
				errorLevel: errorLevel,
				e: e,
				withStackTrace: withStackTrace,
				justFilterableLogValues: false,
				additionalLogValues: additionalLogValues,
				formattedText: formattedText,
				formattedTextParams: formattedTextParams);

			if (LoggWriter.IsActive(logValues))
				LoggWriter.Write2LogFile(logValues);
		}
		
		#endregion
	}
}
