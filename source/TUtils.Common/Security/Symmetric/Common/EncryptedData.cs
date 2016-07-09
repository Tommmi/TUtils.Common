using System;
using TUtils.Common.Extensions;

namespace TUtils.Common.Security.Symmetric.Common
{
	[Serializable]
	public class EncryptedData
	{
		public byte[] Data { get; }
		// ReSharper disable once InconsistentNaming
		public byte[] IV{ get; }

		public EncryptedData(byte[] data, byte[] iv)
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