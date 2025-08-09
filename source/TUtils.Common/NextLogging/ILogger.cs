using System;

namespace TUtils.Common.Logging
{
	public interface ILogger
	{
		void LogInfo(Func<object> map = null);
		void LogWarn(Func<object> map = null);
		void LogError(Func<object> map = null, Exception e = null);
	}
}