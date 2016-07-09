using System;
using System.Linq.Expressions;
using TUtils.Common.Extensions;

namespace TUtils.Common.Logging.Common
{
	public class LoggingFilterableValue : ILoggingValueKey
	{

		public Guid Guid { get; private set; }
		public string ElementName{ get; private set; }
		public bool IsFilterable
		{
			get;
			private set;
		}

		public LoggingFilterableValue Init(Guid guid, Expression<Func<ILoggingValueKey>> guidName, bool isFilterable)
		{
			Guid = guid;
			ElementName = guidName.GetMemberName();
			IsFilterable = isFilterable;
			return this;
		}

	}
}