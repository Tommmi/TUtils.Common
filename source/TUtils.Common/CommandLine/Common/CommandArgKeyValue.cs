using System;
using System.Collections.Generic;

namespace TUtils.Common.CommandLine.Common
{
	public abstract class CommandArgKeyValue<TValuetype> : CommandArgKeyValueBase
	{
		public new KeyValuePair<string, TValuetype> KeyValue { get; private set; }

		private Func<string, TValuetype> _converter;

		protected override void Init(string rawArg)
		{
			throw new ApplicationException("DEPRECATED");
		}

		protected void Init(Func<string, TValuetype> converter, string rawArg)
		{
			base.Init(rawArg);

			_converter = converter;

			KeyValue = new KeyValuePair<string, TValuetype>(
				base.KeyValue.Key,
				_converter(base.KeyValue.Value));
		}

		public override object Value => KeyValue.Value;
	}
}