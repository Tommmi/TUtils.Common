using System.Collections.Generic;

namespace TUtils.Common.CommandLine.Common
{
	public class CommandLineParseResult
	{
		public bool Succeeded { get; private set; }
		// ReSharper disable once MemberCanBePrivate.Global
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public IEnumerable<string> FailedCommandArgs { get; private set; }

		public CommandLineParseResult Init(bool succeeded,IEnumerable<string> failedCommandArgs)
		{
			Succeeded = succeeded;
			FailedCommandArgs = failedCommandArgs;
			return this;
		}
	}
}