namespace TUtils.Common.Logging
{
	public interface ILoggerFactory
	{
		ILogger Create(object loggingInstance, ICallerContext callerContext);
	}
}