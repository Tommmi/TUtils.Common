using System.Text;

namespace TUtils.Common.Logging
{
	public class CallerKontextKeyValuePair
	{
		public string Key { get; set; }
		public string Value { get; set; }

		public CallerKontextKeyValuePair()
		{

		}
		public CallerKontextKeyValuePair(string key, string value)
		{
			Key = key;
			Value = value;
		}
	}
}
