using System;

namespace TUtils.Common.CommandLine.Common
{
	public  class CommandArgString : CommandArgKeyValueBase
	{
		// ReSharper disable once MemberCanBePrivate.Global
		public new CommandArgString Init(string rawArg)
		{
			base.Init(rawArg);
			return this;
		}

		public override object Value => KeyValue.Value;

		public override bool TryInit(string arg)
		{
			try
			{
				Init(arg);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}