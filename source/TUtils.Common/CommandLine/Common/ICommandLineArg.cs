namespace TUtils.Common.CommandLine.Common
{
	public interface ICommandLineArg
	{
		string Key { get; }
		object Value { get; }
		bool TryInit(string arg);
	}
}