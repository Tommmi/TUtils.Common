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

	
	public class Log4NetWriter : LogWriterBase, ILogWriter
	{
		#region fields

		private readonly Dictionary<string, ILog> cachedLoggers = new Dictionary<string, ILog>();

		#endregion

		public Log4NetWriter()
		{
			try
			{
				log4net.Config.XmlConfigurator.Configure();
			}
			catch (Exception e)
			{
				if (e.Message.Contains("The file is not currently locked"))
					throw new ApplicationException("usah38rh may be log output file is opened", e);

				throw;
			}
		}


		#region private

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
