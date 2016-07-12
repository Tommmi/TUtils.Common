using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TUtils.Common.Extensions;
using TUtils.Common.Logging.Common;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace TUtils.Common.Logging.Log4Net
{
	public class Log4NetWriter : ILogWriter
	{
		#region static

		private static readonly object _syncObj;

		/// <summary>
		/// list of known log value keys
		/// </summary>
		private static readonly List<Guid> _columns;

		static Log4NetWriter()
		{
			_syncObj = new object();
			_columns = new List<Guid>
			{
				PredefinedLoggingValueIDs.Timestamp.Guid,
				PredefinedLoggingValueIDs.ThreadName.Guid,
				PredefinedLoggingValueIDs.LoggingText.Guid,
				PredefinedLoggingValueIDs.Filename.Guid,
				PredefinedLoggingValueIDs.Line.Guid,
				PredefinedLoggingValueIDs.MethodName.Guid,
				PredefinedLoggingValueIDs.Namespace.Guid,
				PredefinedLoggingValueIDs.Severity.Guid,
				PredefinedLoggingValueIDs.ExceptionObject.Guid
			};
			//_columns.Add(MPredefinedLoggingValueIDs.ServiceName.GUID);
			//_columns.Add(MPredefinedLoggingValueIDs.CurrentMessageContent.GUID);
			//_columns.Add(MPredefinedLoggingValueIDs.CurrentMessageID.GUID);
			//_columns.Add(MPredefinedLoggingValueIDs.OriginalMessageContent.GUID);
			//_columns.Add(MPredefinedLoggingValueIDs.OriginalMessageID.GUID);
			//_columns.Add(MPredefinedLoggingValueIDs.SendingMessageContent.GUID);
			//_columns.Add(MPredefinedLoggingValueIDs.SendingMessageID.GUID);

			try
			{
				log4net.Config.XmlConfigurator.Configure();			
			}
			catch (Exception e)
			{
				if ( e.Message.Contains("The file is not currently locked"))
					throw new ApplicationException("usah38rh may be log output file is opened",e);

				throw;
			}
		}

		#endregion

		#region fields

		private readonly Dictionary<string, ILog> cachedLoggers = new Dictionary<string, ILog>();

		#endregion

		#region private

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

		private ILog GetLogger(string category)
		{
			lock (_syncObj)
			{
				ILog logger;
				if (cachedLoggers == null)
					throw new NullReferenceException("jdnf83gog3589");
				if (!cachedLoggers.TryGetValue(category, out logger))
				{
					logger = LogManager.GetLogger(category);
					if (category.IsNullOrEmpty())
						throw new NullReferenceException("skfdjv83w487flwhef");
					if (logger == null)
						throw new NullReferenceException("js83tahdueks7eg");

					cachedLoggers[category] = logger;
				}
				return logger;
			}
		}

		private bool GetLoggerCategoryAndSeverity(
			Dictionary<Guid, ILogValue> logValues, 
			out LogSeverityEnum severity,
			out string category,
			out ILog logger)
		{
			logger = null;
			severity = LogSeverityEnum.INFO;
			if (!TryGetValue(logValues, PredefinedLoggingValueIDs.Namespace, out category))
				return false;
			string severityText;
			if (!TryGetValue(logValues, PredefinedLoggingValueIDs.Severity, out severityText))
				return false;

			if ( !Enum.TryParse(severityText,true,out severity))
				return false;
			logger = GetLogger(category);
			if (logger == null)
				return false;
			return true;
		}

		private string GetLogTextInExcelStyle(Dictionary<Guid, ILogValue> logValues)
		{
			lock (_syncObj)
			{
				var unknownLogValues = logValues.Where(logValue => !_columns.Contains(logValue.Key))
												.Select(logValue => logValue.Key);
				_columns.AddRange(unknownLogValues);
				var text = new StringBuilder();
				bool firstTime = true;
				foreach (var colGuid in _columns)
				{
					if (firstTime)
						firstTime = false;
					else
						text.Append(";");

					ILogValue logValue;
					if (logValues.TryGetValue(colGuid, out logValue))
					{
						var key = logValue.Key;

						if (
							colGuid == PredefinedLoggingValueIDs.Timestamp.Guid ||
							colGuid == PredefinedLoggingValueIDs.Severity.Guid ||
							colGuid == PredefinedLoggingValueIDs.Filename.Guid)
							text.Append(logValue.Value);
						else
							text.Append($"{key.ElementName}={logValue.Value}"
								.CleanFromExcelSymbols());
					}
					else
					{
						text.Append(" ");
					}
				}

				return text.ToString();
			}
		}

		#endregion

		#region ILogWriter

		bool ILogWriter.IsActive(Dictionary<Guid, ILogValue> logValues)
		{
			string category;
			LogSeverityEnum severity;
			ILog logger;
			if (!GetLoggerCategoryAndSeverity(
				logValues,
				out severity,
				out category,
				out logger))
			{
				return false;
			}

			switch (severity)
			{
				case LogSeverityEnum.ERROR:
					return logger.IsErrorEnabled;
				case LogSeverityEnum.WARNING:
					return logger.IsWarnEnabled;
				case LogSeverityEnum.INFO:
					return logger.IsInfoEnabled;
				default:
					throw new ArgumentOutOfRangeException("jhsdf74873rn " + severity);
			}
		}

		void ILogWriter.Write2LogFile(Dictionary<Guid, ILogValue> logValues)
		{
			string category;
			LogSeverityEnum severity;
			ILog logger;
			if (!GetLoggerCategoryAndSeverity(
				logValues,
				out severity,
				out category,
				out logger))
			{
				return;
			}

			switch (severity)
			{
				case LogSeverityEnum.ERROR:
					if ( !logger.IsErrorEnabled )
						return;
					break;
				case LogSeverityEnum.WARNING:
					if (!logger.IsWarnEnabled)
						return;
					break;
				case LogSeverityEnum.INFO:
					if (!logger.IsInfoEnabled)
						return;
					break;
				default:
					throw new ArgumentOutOfRangeException("jhsdf74873rn " + severity);
			}

			var text = GetLogTextInExcelStyle(logValues);

			switch (severity)
			{
				case LogSeverityEnum.ERROR:
					logger.Error(text);
					break;
				case LogSeverityEnum.WARNING:
					logger.Warn(text);
					break;
				case LogSeverityEnum.INFO:
					logger.Info(text);
					break;
				default:
					throw new ArgumentOutOfRangeException("hz756ezhjgr44 " + severity);
			}
		}

		#endregion
	}
}
