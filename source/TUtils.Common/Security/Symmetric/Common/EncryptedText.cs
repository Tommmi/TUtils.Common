using System;
using TUtils.Common.Extensions;

namespace TUtils.Common.Security.Symmetric.Common
{
	[Serializable]
	public class EncryptedText
	{
		public byte[] Data { get; set; }
		public byte[] IV { get; }

		public EncryptedText(byte[] data, byte[] iv)
		{
			Data = data;
			IV = iv;
		}

		public string ToBase64String()
		{
			return Data.ToBase64String();
		}
	}
}