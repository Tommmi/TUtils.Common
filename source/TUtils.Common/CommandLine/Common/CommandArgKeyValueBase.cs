using System;
using System.Collections.Generic;
using System.Linq;
using TUtils.Common.Extensions;

namespace TUtils.Common.CommandLine.Common
{
	public abstract class CommandArgKeyValueBase : CommandArgBase
	{
		public KeyValuePair<string, string> KeyValue { get; private set; }

		protected override void Init(string rawArg)
		{
			foreach (var splitChar in new[]{'=',':'})
			{
				var parts = rawArg.Split(splitChar);
				if (parts.Length == 2 && !parts.Any(p => p.IsNullOrEmpty<char>()))
				{
					KeyValue = new KeyValuePair<string, string>(parts[0], parts[1]);
					base.Init(rawArg);
					return;
				}
			}

			throw new ArgumentException("6wefjhjksa93h " + rawArg);
		}

		public override string Key
		{
			get { return KeyValue.Key; }
		}
	}
}