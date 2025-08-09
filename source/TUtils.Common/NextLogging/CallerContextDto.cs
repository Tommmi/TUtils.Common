using System.Collections.Generic;

namespace TUtils.Common.Logging
{
	public class CallerContextDto
	{
		public List<CallerKontextKeyValuePair> KeyValues { get; set; }

		public CallerContextDto()
		{
		}

		public CallerContextDto(IEnumerable<CallerKontextKeyValuePair> keyValuePairs)
		{
			KeyValues = new List<CallerKontextKeyValuePair>();
			KeyValues.AddRange(keyValuePairs);
		}
	}
}