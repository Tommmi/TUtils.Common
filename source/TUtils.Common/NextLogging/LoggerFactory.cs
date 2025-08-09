namespace TUtils.Common.Logging
{
	public class LoggerFactory : ILoggerFactory
	{
		private readonly ITLog _log;

		public LoggerFactory(ITLog log)
		{
			_log = log;
		}

		public ILogger Create(object loggingInstance, ICallerContext callerContext)
		{
			return new Logger(_log, callerContext, loggingInstance);
		}
	}
}