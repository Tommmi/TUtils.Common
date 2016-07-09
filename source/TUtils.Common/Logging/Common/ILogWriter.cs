using System;
using System.Collections.Generic;

namespace TUtils.Common.Logging.Common
{
	/// <summary>
	/// Interface zu einem Logging-Service
	/// </summary>
	public interface ILogWriter
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
		bool IsActive(Dictionary<Guid, ILogValue> logValues);

		///  <summary>
		///  Schreibt die Ausgabe in das Log. 
		///  ErrorLevel und logCaregory werden nicht geprüft.
		///  Wird nur aufgerufen, wenn IsActive(..) true zurückgibt.
		///  </summary>
		void Write2LogFile(Dictionary<Guid, ILogValue> logValues);
	}
}
