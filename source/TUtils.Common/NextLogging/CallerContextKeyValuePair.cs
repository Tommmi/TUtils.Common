using System.Text;

namespace TUtils.Common.Logging
{
	public class CallerContextKeyValuePair
	{
		public string Key { get; set; }
		public string Value { get; set; }

		public CallerContextKeyValuePair()
		{

		}
		public CallerContextKeyValuePair(string key, string value)
		{
			Key = key;
			Value = value;
		}
	}
}
