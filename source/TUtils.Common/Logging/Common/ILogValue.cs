namespace TUtils.Common.Logging.Common
{
	/// <summary>
	/// Represents a loggable part of a logging entry.
	/// Typically an ILogValue will be written into a logging file.
	/// If for example the logging file is an excel table, ILogValue
	/// will be written into a cell of that file. A row represents
	/// a single logging entry. ILogValue.Key.GUID identifies
	/// a column of the table. ILogValue.Key.ElementName rperesents the 
	/// header of the column. ILogValue.Value represents the content of the cell.
	/// </summary>
	public interface ILogValue
	{
		ILoggingValueKey Key { get; }
		string Value
		{
			get;
		}
	}

}