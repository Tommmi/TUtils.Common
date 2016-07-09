using TUtils.Common.Extensions;

namespace TUtils.Common.Security.Symmetric.Common
{
	public class PlainText 
	{
		public string Text { get; }

		public PlainText(string text)
		{
			Text = text;
		}

		public byte[] AsUTF8EncodedByteArray()
		{
			return Text.ToUTF8CodedByteArray();
		}
	}
}