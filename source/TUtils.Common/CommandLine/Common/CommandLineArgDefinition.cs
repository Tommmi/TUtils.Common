using System;

namespace TUtils.Common.CommandLine.Common
{
	public class CommandLineArgDefinition : CommandLineArgDefinitionBase
	{
		public new CommandLineArgDefinition Init(
			Func<ICommandLineArg> createCommandLineArg,
			string key)
		{
			base.Init(createCommandLineArg, key);
			return this;
		}
	}
}