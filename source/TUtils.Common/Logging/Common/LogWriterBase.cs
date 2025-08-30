using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUtils.Common.Extensions;

namespace TUtils.Common.Logging.Common;

public abstract class LogWriterBase
{
	#region static

	protected static readonly object _syncObj;

	/// <summary>
	/// list of known log value keys
	/// </summary>
	protected static readonly List<Guid> _columns;

	static LogWriterBase()
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
			if (e.Message.Contains("The file is not currently locked"))
				throw new ApplicationException("usah38rh may be log output file is opened", e);

			throw;
		}
	}

	#endregion

	protected bool TryGetValue(Dictionary<Guid, ILogValue> logValues, ILoggingValueKey key, out string value)
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

	protected string GetLogTextInExcelStyle(Dictionary<Guid, ILogValue> logValues)
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
}