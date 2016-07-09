using System;
using System.Collections.Generic;
using TUtils.Common.Logging.Common;

namespace TUtils.Common.Logging.LogMocs
{
	// ReSharper disable once UnusedMember.Global
	public class LogMocWriter : ILogWriter
	{
		/// <summary>
		/// True, if the log entry should be logged.
		/// logValues[PredefinedLoggingValueIDs.LoggingText] won't exist for 
		/// IsActive.
		/// </summary>
		/// <param name="logValues">
		/// The filterable values, which are about to be logged now.
		/// For example the the thread id, the namespace where logging was caused
		/// and so on are filterable.
		/// logValues is a map:
		/// Key: catogory of the logged value. There are following
		///      Guids predefined: see all Guids, which has attribute "LoggingFilterableValueAttribute".
		///		 For example class "PredefinedLoggingValueIDs".
		/// Value: An object, which implements interface "ILogValue".
		/// </param>
		/// <returns></returns>
		public bool IsActive(Dictionary<Guid, ILogValue> logValues)
		{
			return false;
		}

		///  <summary>
		///  Schreibt die Ausgabe in das Log. 
		///  ErrorLevel und logCaregory werden nicht geprüft.
		///  Wird nur aufgerufen, wenn IsActive(..) true zurückgibt.
		///  </summary>
		public void Write2LogFile(Dictionary<Guid, ILogValue> logValues)
		{
			
		}
	}
}
