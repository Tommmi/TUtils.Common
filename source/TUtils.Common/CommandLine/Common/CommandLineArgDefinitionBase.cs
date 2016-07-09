using System;
// ReSharper disable UnusedMember.Global

namespace TUtils.Common.CommandLine.Common
{
	public abstract class CommandLineArgDefinitionBase
	{
		public string Key { get; private set; }
		private Func<ICommandLineArg> _createCommandLineArg;

		public bool TryGetCommandLineArg(string arg, out ICommandLineArg argObj)
		{
			argObj = _createCommandLineArg();
			if (argObj.TryInit(arg))
				return argObj.Key == Key;
			return false;
		}

		protected void Init(Func<ICommandLineArg> createCommandLineArg, string key)
		{
			Key = key;
			_createCommandLineArg = createCommandLineArg;
		}

		// ReSharper disable once VirtualMemberNeverOverriden.Global
		public virtual string UsageHint => $"{Key}=<value>";
	}
}