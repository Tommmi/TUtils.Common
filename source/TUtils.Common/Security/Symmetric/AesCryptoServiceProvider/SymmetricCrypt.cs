using System;
using System.IO;
using System.Security.Cryptography;
using TUtils.Common.Extensions;
using TUtils.Common.Security.Symmetric.Common;

namespace TUtils.Common.Security.Symmetric.AesCryptoServiceProvider
{
	public class SymmetricCrypt : ISymmetricCrypt
	{
		private readonly Guid _publicId;
		private readonly SymmetricSecret _secret;
		private System.Security.Cryptography.AesCryptoServiceProvider _dataencrypt;

		public SymmetricCrypt(SymmetricSecret secret)
		{
			_publicId = Guid.NewGuid();
			_dataencrypt = new System.Security.Cryptography.AesCryptoServiceProvider();

			//Block size : Gets or sets the block size, in bits, of the cryptographic operation.  
			_dataencrypt.BlockSize = 128;
			//KeySize: Gets or sets the size, in bits, of the secret key  
			_dataencrypt.KeySize = 128;
			//Padding: Gets or sets the padding mode used in the symmetric algorithm  
			_dataencrypt.Padding = PaddingMode.PKCS7;
			//Mode: Gets or sets the mode for operation of the symmetric algorithm  
			_dataencrypt.Mode = CipherMode.CBC;

			_dataencrypt.GenerateIV();

			if (secret == null)
			{
				_dataencrypt.GenerateKey();
				_secret = new SymmetricSecret(_dataencrypt.Key);
			}
			else
			{
				_secret = secret;
				_dataencrypt.Key = _secret.Key;
			}
		}

		public SymmetricCrypt() : this(null)
		{

		}

		void IDisposable.Dispose()
		{
			_dataencrypt?.Clear();
			_dataencrypt = null;
		}

		Guid ISymmetricCrypt.PublicId => _publicId;

		ISymmetricSecret ISymmetricCrypt.Secret => _secret;

		EncryptedData ISymmetricCrypt.Encrypt(PlainData plainData)
		{
			var bytes = plainData.Data;
			_dataencrypt.GenerateIV();
			using (var encryptor = _dataencrypt.CreateEncryptor(_secret.Key, _dataencrypt.IV))
			using (MemoryStream msEncrypt = new MemoryStream())
			{
				using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
				{
					csEncrypt.Write(bytes, 0, bytes.Length);
					csEncrypt.FlushFinalBlock();
					return new EncryptedData(msEncrypt.ToArray(), _dataencrypt.IV);
				}
			}
		}

		EncryptedText ISymmetricCrypt.Encrypt(PlainText plainText)
		{
			var encryptedData = (this as ISymmetricCrypt).Encrypt(new PlainData(plainText.AsUTF8EncodedByteArray()));
			return new EncryptedText(
				encryptedData.Data,
				encryptedData.IV);
		}

		PlainText ISymmetricCrypt.Decrypt(EncryptedText encryptedText)
		{
			return new PlainText(
				(this as ISymmetricCrypt)
					.Decrypt(new EncryptedData(encryptedText.Data, encryptedText.IV))
					.Data
					.ToStringFromUTF8CodedByteArray());
		}

		PlainData ISymmetricCrypt.Decrypt(EncryptedData encryptedData)
		{
			//Creates a symmetric AES encryptor object using the current key and initialization vector (IV).  
			using (var decryptor = _dataencrypt.CreateDecryptor(_secret.Key, encryptedData.IV))
			using (MemoryStream msDecrypt = new MemoryStream(encryptedData.Data))
			{
				using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				{
					using (MemoryStream srDecrypt = new MemoryStream())
					{
						csDecrypt.CopyTo(srDecrypt);
						return new PlainData(srDecrypt.ToArray());
					}
				}
			}
		}
	}
}