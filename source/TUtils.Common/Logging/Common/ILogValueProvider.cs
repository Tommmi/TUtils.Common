using System.Collections.Generic;

namespace TUtils.Common.Logging.Common
{
	public interface ILogValueProvider
	{
		IEnumerable<ILogValue> GetLogValues(
			object loggingInstance,
			string strNamespace);
	}
}
