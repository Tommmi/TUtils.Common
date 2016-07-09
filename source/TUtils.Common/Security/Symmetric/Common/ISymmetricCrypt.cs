using System;

namespace TUtils.Common.Security.Symmetric.Common
{

	public interface ISymmetricCrypt : IDisposable
	{
		Guid PublicId { get; }
		ISymmetricSecret Secret { get; }
		EncryptedText Encrypt(PlainText plainText);
		PlainText Decrypt(EncryptedText encryptedText);
		PlainData Decrypt(EncryptedData encryptedData);
		EncryptedData Encrypt(PlainData plainData);
	}
}