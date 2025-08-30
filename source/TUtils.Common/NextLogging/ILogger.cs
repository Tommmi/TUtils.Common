using System;
using System.Runtime.CompilerServices;

namespace TUtils.Common.Logging
{
	public interface ILogger
	{
		void LogInfo(
				Func<object> map = null, 
				[CallerMemberName] string memberName = "",
				[CallerLineNumber] int lineNumber = 0);
		void LogWarn(Func<object> map = null,
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int lineNumber = 0);
		void LogError(Func<object> map = null, Exception e = null,
			[CallerMemberName] string memberName = "",
			[CallerLineNumber] int lineNumber = 0);
	}
}