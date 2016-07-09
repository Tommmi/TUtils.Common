using System.IO;

namespace TUtils.Common.CommandLine.Common
{
	public class CommandArgFilePath : CommandArgString
	{
		public override bool TryInit(string arg)
		{
			if (!base.TryInit(arg))
				return false;
			return File.Exists(KeyValue.Value);
		}
	}
}