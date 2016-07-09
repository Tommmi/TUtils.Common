using TUtils.Common.Extensions;

namespace TUtils.Common.Security.Symmetric.Common
{
	public class PlainData
	{
		public byte[] Data { get; }

		public PlainData(byte[] data)
		{
			Data = data;
		}

		public string AsBase64String()
		{
			return Data.ToBase64String();
		}
	}
}