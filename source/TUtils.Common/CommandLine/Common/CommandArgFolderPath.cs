using System.IO;

namespace TUtils.Common.CommandLine.Common
{
	public class CommandArgFolderPath : CommandArgString
	{
		public override bool TryInit(string arg)
		{
			if (!base.TryInit(arg))
				return false;
			return Directory.Exists(KeyValue.Value);
		}
	}
}