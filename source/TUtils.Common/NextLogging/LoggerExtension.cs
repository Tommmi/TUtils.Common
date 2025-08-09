namespace TUtils.Common.Logging
{
	public static class LoggerExtension
	{
		public static ILogger Log(this object loggingInstance, ICallerContext callContext)
		{
			return callContext.LoggerFactory.Create(loggingInstance, callContext);
		}
	}
}