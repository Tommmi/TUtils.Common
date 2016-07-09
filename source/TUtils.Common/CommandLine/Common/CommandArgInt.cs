using System;
// ReSharper disable UnusedMember.Global

namespace TUtils.Common.CommandLine.Common
{
	public class CommandArgInt : CommandArgKeyValue<int>
	{
		private static int Convert(string arg)
		{
			return int.Parse(arg);
		}

		// ReSharper disable once MemberCanBePrivate.Global
		// ReSharper disable once UnusedMethodReturnValue.Global
		public new CommandArgInt Init(string rawArg)
		{
			base.Init(Convert, rawArg);
			return this;
		}

		public new int Value => KeyValue.Value;

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