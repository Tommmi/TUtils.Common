using System.Collections.Generic;

namespace TUtils.Common.Logging
{
	public class CallerContextDto
	{
		public List<CallerContextKeyValuePair> KeyValues { get; set; }

		public CallerContextDto()
		{
		}

		public CallerContextDto(IEnumerable<CallerContextKeyValuePair> keyValuePairs)
		{
			KeyValues = new List<CallerContextKeyValuePair>();
			KeyValues.AddRange(keyValuePairs);
		}
	}
}