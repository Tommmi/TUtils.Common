using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using TUtils.Common.Extensions;
using TUtils.Common.Security.Asymmetric.Common;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

namespace TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider
{
	public static class RsaLib
	{
		#region public certificate

		/// <summary>
		/// loads public certification from *.cer file.
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="filePath">
		/// path to *.cer file, containing the public key
		/// (for example created by makecert.exe -iv CA.pvk -n "CN=CER_1" -ic CA.cer -sv CER_1.pvk CER_1.cer -sky exchange -pe -a sha256 –len 2048)
		/// </param>
		/// <param name="password">may be null, if certificate isn't password-protected</param>
		/// <returns>always != null</returns>
		public static PublicCertificate GetPublicCertificateByFilePath(string filePath, string password)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException(filePath);
			if (password == null)
				return new PublicCertificate(new X509Certificate2(filePath));
			return new PublicCertificate(new X509Certificate2(filePath, password));
		}

		/// <summary>
		/// Gets public certificate
		/// You must dispose returned certificate by yourself !
		/// 
		/// </summary>
		/// <param name="certContentBase64String"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static PublicCertificate GetPublicCertificate(this PublicCertContentBase64String certContentBase64String, string password)
		{
			var data = certContentBase64String.Content.ToByteArrayFromBase64String();

			if (password == null)
				return new PublicCertificate(new X509Certificate2(data));
			return new PublicCertificate(new X509Certificate2(data, password));
		}

		public static PublicCertContentBase64String ToBase64String(this PublicCertificate publicCertificate)
		{
			return new PublicCertContentBase64String(publicCertificate.Cert.RawData.ToBase64String());
		}

		public static PublicCertificate ToPublicCertificate(this PrivateCertificate privateCertificate)
		{
			return new PublicCertificate(new X509Certificate2(privateCertificate.Cert.RawData));
		}

		/// <summary>
		/// finds public certification in windows certification storage by friendly name
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="subjectName">
		/// CN={subjectName}
		/// </param>
		/// <returns></returns>
		public static PublicCertificate GetPublicCertificateFromWindowsStorage(string subjectName)
		{
			var cert = GetCertificateFromWindowsStorage(subjectName);
			if (cert is PublicCertificate)
				return cert as PublicCertificate;
			return new PublicCertificate(cert.Cert);
		}

		#endregion

		#region private certificate

		/// <summary>
		/// loads public certification from *.cer file and looks for 
		/// matching private certification in windows certification store
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="filePath">
		/// path to *.cer file, containing the public key
		/// </param>
		/// <param name="password">may be null, if certificate isn't password-protected</param>
		/// <returns>may be null</returns>
		/// <exception cref="FileNotFoundException"></exception>
		public static PrivateCertificate GetPrivateCertificateByFilePath(string filePath, string password)
		{
			return GetPublicCertificateByFilePath(filePath, password).GetPrivateCertificateFromWindowsStorage();
		}

		/// <summary>
		/// gets private certificate by base64 string represenation.
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <summary>
		/// Note ! You can get base64 string represenation of a private certification by calling
		///		RsaLib.GetPublicCertificateByFilePath(filePathToPublicCer,"myPassword")
		///		.GetPrivateCertificateFromWindowsStorage()
		///		.ToBase64String();
		/// </summary>
		/// <param name="certContentBase64String"></param>
		/// <returns></returns>
		public static PrivateCertificate GetPrivateCertificate(this PrivateCertContentBase64String certContentBase64String)
		{
			var data = certContentBase64String.Content.ToByteArrayFromBase64String();
			return new PrivateCertificate(new X509Certificate2(data));
		}

		/// <summary>
		/// Looks for matching private certificate in windows certification storage
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="publicCertBase64"></param>
		/// <param name="password"></param>
		/// <returns>may be null</returns>
		public static PrivateCertificate GetPrivateCertificate(this PublicCertContentBase64String publicCertBase64, string password)
		{
			return publicCertBase64.GetPublicCertificate(password).GetPrivateCertificateFromWindowsStorage();
		}

		public static PrivateCertContentBase64String ToBase64String(this PrivateCertificate privatCertificate)
		{
			return new PrivateCertContentBase64String(privatCertificate.Cert.Export(X509ContentType.Pfx).ToBase64String());
		}

		/// <summary>
		/// Looks for a private certificate matching the passed public certificate
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <returns></returns>
		public static PrivateCertificate GetPrivateCertificateFromWindowsStorage(this PublicCertificate publicCertificate)
		{
			X509Store store = new X509Store(StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadOnly);
			var certHash = publicCertificate.Cert.GetCertHashString();
			var cert = store.Certificates
				.Cast<X509Certificate2>()
				.Where(c => c.GetCertHashString() == certHash)
				.FirstOrDefault(c => c.HasPrivateKey);
			if (cert == null)
				return null;
			return new PrivateCertificate(cert);
		}

		/// <summary>
		/// Looks for private certificate in windows certification storage by friendly name.
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="subjectName">
		/// CN={subjectName}
		/// </param>
		/// <returns></returns>
		public static IPrivateCertificate GetPrivateCertificateFromWindowsStorage(string subjectName)
		{
			var cert = GetCertificateFromWindowsStorage(subjectName);
			return cert as PrivateCertificate;
		}

		#endregion

		#region Encrypt / Decrypt

		/// <summary>
		/// 
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="encryptedBase64Text">
		/// base 64 string representation of the encrypted byte array of a UTF8-coded text
		/// </param>
		/// <returns>plain readable text</returns>
		public static string Decrypt(this PrivateCertificate privateCertificate, string encryptedBase64Text)
		{
			return privateCertificate.Decrypt(encryptedBase64Text.ToByteArrayFromBase64String()).ToStringFromUTF8CodedByteArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="encryptedData">
		/// encrypted byte array (may contain anything)
		/// </param>
		/// <returns>decrypted byte array (may contain anything)</returns>
		public static byte[] Decrypt(this PrivateCertificate privateCertificate, byte[] encryptedData)
		{
			return EncryptDecrypt(privateCertificate, encryptedData, encrypt: false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="plainText">
		/// must be UTF8 encodeable !
		/// </param>
		/// <returns>
		/// base 64 representation of the encrypted byte array of the UTF8 coded plain text byte array
		/// </returns>
		public static string Encrypt(this PublicCertificate publicCertificate, string plainText)
		{
			return publicCertificate.Encrypt(plainText.ToUTF8CodedByteArray()).ToBase64String();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="plainData">
		/// plain byte array 
		/// </param>
		/// <returns>
		/// encrypted byte array
		/// </returns>
		public static byte[] Encrypt(this PublicCertificate publicCertificate, byte[] plainData)
		{
			return EncryptDecrypt(publicCertificate, plainData, encrypt: true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="plainText">
		/// must be UTF8 encodeable !
		/// </param>
		/// <returns>
		/// base 64 representation of the encrypted byte array of the UTF8 coded plain text byte array
		/// </returns>
		public static string Encrypt(this PrivateCertificate privateCertificate, string plainText)
		{
			return privateCertificate.Encrypt(plainText.ToUTF8CodedByteArray()).ToBase64String();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="plainData">
		/// plain byte array 
		/// </param>
		/// <returns>
		/// </returns>
		public static byte[] Encrypt(this PrivateCertificate privateCertificate, byte[] plainData)
		{
			return EncryptDecrypt(new PublicCertificate(privateCertificate.Cert), plainData, encrypt: true);
		}

		#endregion

		#region signature

		/// <summary>
		/// Creates SHA1 hash from data with Pkcs1 padding and signs it.
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Sign(this PrivateCertificate privateCertificate, byte[] data)
		{
			if (privateCertificate.Rsa == null)
				privateCertificate.Rsa = privateCertificate.GetPrivateRsa();
			var rsa = privateCertificate.Rsa;
			var signature = rsa.SignData(data,HashAlgorithmName.SHA1,RSASignaturePadding.Pkcs1);
			return signature;
		}

		/// <summary>
		/// Creates SHA1 hash from data with Pkcs1 padding and signs it.
		/// Returns Base64 representation of signature.
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string SignBase64(this PrivateCertificate privateCertificate, byte[] data)
		{
			return privateCertificate.Sign(data).ToBase64String();
		}

		/// <summary>
		/// Creates SHA1 hash from UTF8 encoded byte representation of text and signs it.
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static byte[] Sign(this PrivateCertificate privateCertificate, string text)
		{
			return privateCertificate.Sign(text.ToUTF8CodedByteArray());
		}

		/// <summary>
		/// Creates SHA1 hash from UTF8 encoded byte representation of text and signs it.
		/// Returns Base64 representation of signature.
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string SignBase64(this PrivateCertificate privateCertificate, string text)
		{
			return privateCertificate.Sign(text).ToBase64String();
		}

		/// <summary>
		/// Verifies signature with data.
		/// 
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="data"></param>
		/// <param name="signature">
		/// signature of SHA1 hash from data with Pkcs1 padding
		/// </param>
		/// <returns>true if signature and data match</returns>
		public static bool Verify(this PublicCertificate publicCertificate, byte[] data, byte[] signature)
		{
			if (publicCertificate.Rsa == null)
				publicCertificate.Rsa = publicCertificate.GetPublicRsa();
			var rsa = publicCertificate.Rsa;
			return rsa.VerifyData(data, signature, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
		}

		/// <summary>
		/// Verifies signature with data.
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="data"></param>
		/// <param name="signature">
		/// Base64 representation of signature of SHA1 hash from data
		/// </param>
		/// <returns></returns>
		public static bool Verify(this PublicCertificate publicCertificate, byte[] data, string signature)
		{
			return publicCertificate.Verify(data, signature.ToByteArrayFromBase64String());
		}


		/// <summary>
		/// Verifies signature with data.
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="text"></param>
		/// <param name="signature">
		/// signature of SHA1 hash of UTF8 encoded byte representation of text 
		/// </param>
		/// <returns></returns>
		public static bool Verify(this PublicCertificate publicCertificate, string text, byte[] signature)
		{
			return publicCertificate.Verify(text.ToUTF8CodedByteArray(), signature);
		}

		/// <summary>
		/// Verifies signature with data.
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="text"></param>
		/// <param name="signature">
		/// base64 representation of signature of SHA1 hash of UTF8 encoded byte representation of text 
		/// </param>
		/// <returns></returns>
		public static bool Verify(this PublicCertificate publicCertificate, string text, string signature)
		{
			return publicCertificate.Verify(text.ToUTF8CodedByteArray(), signature.ToByteArrayFromBase64String());
		}

		#endregion

		#region get RSA algorithm

		/// <summary>
		/// Returns rsa-algorithm for given public certificate.
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static System.Security.Cryptography.RSACryptoServiceProvider GetPublicRsa(this PublicCertContentBase64String publicCertificate, string password)
		{
			return publicCertificate
				.GetPublicCertificate(password)
				.GetPublicRsa();
		}

		/// <summary>
		/// Returns rsa-algorithm for public certificate file.
		/// </summary>
		/// <param name="filePathToPublicCertificate"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static System.Security.Cryptography.RSACryptoServiceProvider GetPublicRsa(string filePathToPublicCertificate, string password)
		{
			return GetPublicCertificateByFilePath(filePathToPublicCertificate, password)
				.GetPublicRsa();
		}

		public static System.Security.Cryptography.RSACryptoServiceProvider GetPublicRsa(this PublicCertificate publicCertificate)
		{
			return (System.Security.Cryptography.RSACryptoServiceProvider)publicCertificate.Cert.PublicKey.Key;
		}

		/// <summary>
		/// Finds the private key matching the passed public certificate by looking into the windows certification storage.
		/// Returns rsa-algorithm for private key.
		/// </summary>
		/// <param name="filePathToPublicCertificate"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static System.Security.Cryptography.RSACryptoServiceProvider GetPrivateRsa(string filePathToPublicCertificate, string password)
		{
			return GetPublicCertificateByFilePath(filePathToPublicCertificate, password)
				.GetPrivateCertificateFromWindowsStorage()
				.GetPrivateRsa();
		}

		/// <summary>
		/// Finds the private key matching the passed public key by looking into the windows certification storage.
		/// Returns rsa-algorithm for private key.
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static System.Security.Cryptography.RSACryptoServiceProvider GetPrivateRsa(this PublicCertContentBase64String publicCertificate, string password)
		{
			return publicCertificate
				.GetPublicCertificate(password)
				.GetPrivateCertificateFromWindowsStorage()
				.GetPrivateRsa();
		}

		/// <summary>
		/// Returns rsa-algorithm for private key.
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <returns></returns>
		public static System.Security.Cryptography.RSACryptoServiceProvider GetPrivateRsa(this PrivateCertContentBase64String privateCertificate)
		{
			return privateCertificate
				.GetPrivateCertificate()
				.GetPrivateRsa();
		}

		public static System.Security.Cryptography.RSACryptoServiceProvider GetPrivateRsa(this PrivateCertificate privateCertificate)
		{
			return (System.Security.Cryptography.RSACryptoServiceProvider)privateCertificate.Cert.PrivateKey;
		}

		#endregion

		#region private methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subjectName">
		/// CN={subjectName}
		/// </param>
		/// <returns></returns>
		private static Certificate GetCertificateFromWindowsStorage(string subjectName)
		{
			X509Store store = new X509Store(StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
			if (!subjectName.StartsWith("CN="))
				subjectName = $"CN={subjectName}";
			var certificate = store.Certificates
				.Cast<X509Certificate2>()
				.FirstOrDefault(cert => cert.Subject == subjectName);
			if (certificate == null)
				return null;
			if (certificate.HasPrivateKey)
				return new PrivateCertificate(certificate);
			return new PublicCertificate(certificate);
		}

		public static byte[] EncryptDecrypt(this Certificate certificate, byte[] data, bool encrypt)
		{
			if (certificate.Rsa == null)
				certificate.Rsa = encrypt ? ((PublicCertificate)certificate).GetPublicRsa() : ((PrivateCertificate)certificate).GetPrivateRsa();
			var rsa = certificate.Rsa;
			Func<byte[], byte[]> transform = encrypt 
				? (Func<byte[], byte[]>)(d => rsa.Encrypt(d, true)) 
				: d => rsa.Decrypt(d, true);

			var maxByteCount = rsa.KeySize / 8;
			if (encrypt)
				maxByteCount /= 2; // must be much smaller than key size

			byte[] bytesCopied = null;
			byte[] result = null;
			for (int i = 0; i < data.Length; i += maxByteCount)
			{
				if (i + maxByteCount < data.Length)
				{
					if (bytesCopied == null)
						bytesCopied = new byte[maxByteCount];
					Array.Copy(data, i, bytesCopied, 0, maxByteCount);
					var encryptesBytes = transform(bytesCopied);
					if (result == null)
						result = encryptesBytes;
					else
						result = result.ConcatBytes(encryptesBytes);
				}
				else
				{
					if (result == null)
						return transform(data);
					bytesCopied = new byte[data.Length - i];
					Array.Copy(data, i, bytesCopied, 0, bytesCopied.Length);
					var encryptesBytes = transform(bytesCopied);
					return result.ConcatBytes(encryptesBytes);
				}
			}
			return null; // may not reach this code
		}

		#endregion
	}
}
