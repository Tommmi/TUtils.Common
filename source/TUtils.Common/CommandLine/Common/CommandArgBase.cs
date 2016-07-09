namespace TUtils.Common.CommandLine.Common
{
	public abstract class CommandArgBase : ICommandLineArg
	{
		// ReSharper disable once UnusedAutoPropertyAccessor.Global
		// ReSharper disable once MemberCanBePrivate.Global
		public string RawArg { get; private set; }

		protected virtual void Init(string rawArg)
		{
			RawArg = rawArg;
		}

		public abstract string Key { get; }
		public abstract object Value { get; }


		public abstract bool TryInit(string arg);
	}
}