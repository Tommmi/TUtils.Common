using System;

namespace TUtils.Common.Security.Symmetric.Common
{
	[Serializable]
	public class SymmetricSecret : ISymmetricSecret
	{
		public SymmetricSecret(byte[] key)
		{
			Key = key;
		}

		public byte[] Key { get; }
	}
}