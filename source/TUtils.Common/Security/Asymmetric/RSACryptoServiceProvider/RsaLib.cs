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
		/// <exception cref="ArgumentException">If filePath is null or empty</exception>
		/// <exception cref="FileNotFoundException">If certificate file does not exist</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">If certificate is invalid or password is incorrect</exception>
		public static PublicCertificate GetPublicCertificateByFilePath(string filePath, string password)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

			if (!File.Exists(filePath))
				throw new FileNotFoundException($"Certificate file not found.", filePath);

			try
			{
				X509Certificate2 cert;
				if (password == null)
				{
					cert = new X509Certificate2(filePath);
				}
				else
				{
					cert = new X509Certificate2(filePath, password);
				}
				return new PublicCertificate(cert);
			}
			catch (System.Security.Cryptography.CryptographicException ex)
			{
				throw new System.Security.Cryptography.CryptographicException(
					"Failed to load certificate. Please verify the file format and password.", ex);
			}
			catch (Exception ex)
			{
				throw new System.Security.Cryptography.CryptographicException(
					"An unexpected error occurred while loading the certificate.", ex);
			}
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
		/// <exception cref="ArgumentException">If subjectName is null or empty</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">If certificate not found or access denied</exception>
		public static PublicCertificate GetPublicCertificateFromWindowsStorage(string subjectName)
		{
			if (string.IsNullOrWhiteSpace(subjectName))
				throw new ArgumentException("Subject name cannot be null or empty.", nameof(subjectName));

			try
			{
				var cert = GetCertificateFromWindowsStorage(subjectName);
				if (cert == null)
					throw new System.Security.Cryptography.CryptographicException($"Certificate with subject '{subjectName}' not found in Windows storage.");
				
				if (cert is PublicCertificate publicCert)
					return publicCert;
				return new PublicCertificate(cert.Cert);
			}
			catch (System.Security.Cryptography.CryptographicException)
			{
				throw; // Re-throw crypto exceptions as-is
			}
			catch (Exception ex)
			{
				throw new System.Security.Cryptography.CryptographicException(
					$"Failed to retrieve certificate '{subjectName}' from Windows storage.", ex);
			}
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

		public static PrivateCertificate GetPrivateCertificate(this PrivateCertContentBase64String certContentBase64String, string password)
		{
			var data = certContentBase64String.Content.ToByteArrayFromBase64String();
			if (password == null)
				return new PrivateCertificate(new X509Certificate2(data));
			return new PrivateCertificate(new X509Certificate2(data, password));
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
		/// <exception cref="ArgumentNullException">If publicCertificate is null</exception>
		/// <exception cref="ObjectDisposedException">If publicCertificate is disposed</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">If store access fails</exception>
		public static PrivateCertificate GetPrivateCertificateFromWindowsStorage(this PublicCertificate publicCertificate)
		{
			if (publicCertificate == null)
				throw new ArgumentNullException(nameof(publicCertificate));

			publicCertificate.ThrowIfDisposed();

			using (var store = new X509Store(StoreLocation.LocalMachine))
			{
				try
				{
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
				catch (System.Security.Cryptography.CryptographicException)
				{
					throw; // Re-throw crypto exceptions as-is
				}
				catch (Exception ex)
				{
					throw new System.Security.Cryptography.CryptographicException(
						"Failed to access Windows certificate store for private key lookup.", ex);
				}
			}
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
		/// Creates SHA256 hash from data with PSS padding and signs it.
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">If privateCertificate or data is null</exception>
		/// <exception cref="ObjectDisposedException">If privateCertificate is disposed</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">If signing fails</exception>
		public static byte[] Sign(this PrivateCertificate privateCertificate, byte[] data)
		{
			if (privateCertificate == null)
				throw new ArgumentNullException(nameof(privateCertificate));
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			privateCertificate.ThrowIfDisposed();

			try
			{
				if (privateCertificate.Rsa == null)
					privateCertificate.Rsa = privateCertificate.GetPrivateRsa();
				var rsa = privateCertificate.Rsa;
				var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
				return signature;
			}
			catch (System.Security.Cryptography.CryptographicException)
			{
				throw; // Re-throw crypto exceptions as-is
			}
			catch (Exception ex)
			{
				throw new System.Security.Cryptography.CryptographicException(
					"An error occurred during the signing operation.", ex);
			}
		}

		/// <summary>
		/// Creates SHA256 hash from data with PSS padding and signs it.
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
		/// Creates SHA256 hash from UTF8 encoded byte representation of text and signs it.
		/// </summary>
		/// <param name="privateCertificate"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static byte[] Sign(this PrivateCertificate privateCertificate, string text)
		{
			return privateCertificate.Sign(text.ToUTF8CodedByteArray());
		}

		/// <summary>
		/// Creates SHA256 hash from UTF8 encoded byte representation of text and signs it.
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
		/// signature of SHA256 hash from data with PSS padding
		/// </param>
		/// <returns>true if signature and data match</returns>
		/// <exception cref="ArgumentNullException">If any parameter is null</exception>
		/// <exception cref="ObjectDisposedException">If publicCertificate is disposed</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">If verification fails</exception>
		public static bool Verify(this PublicCertificate publicCertificate, byte[] data, byte[] signature)
		{
			if (publicCertificate == null)
				throw new ArgumentNullException(nameof(publicCertificate));
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (signature == null)
				throw new ArgumentNullException(nameof(signature));

			publicCertificate.ThrowIfDisposed();

			try
			{
				if (publicCertificate.Rsa == null)
					publicCertificate.Rsa = publicCertificate.GetPublicRsa();
				var rsa = publicCertificate.Rsa;
				return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
			}
			catch (System.Security.Cryptography.CryptographicException)
			{
				throw; // Re-throw crypto exceptions as-is
			}
			catch (Exception ex)
			{
				throw new System.Security.Cryptography.CryptographicException(
					"An error occurred during the signature verification.", ex);
			}
		}

		/// <summary>
		/// Verifies signature with data.
		/// </summary>
		/// <param name="publicCertificate"></param>
		/// <param name="data"></param>
		/// <param name="signature">
		/// Base64 representation of signature of SHA256 hash from data
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
		/// signature of SHA256 hash of UTF8 encoded byte representation of text 
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
		/// base64 representation of signature of SHA256 hash of UTF8 encoded byte representation of text 
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
		public static System.Security.Cryptography.RSA GetPublicRsa(this PublicCertContentBase64String publicCertificate, string password)
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
		public static System.Security.Cryptography.RSA GetPublicRsa(string filePathToPublicCertificate, string password)
		{
			return GetPublicCertificateByFilePath(filePathToPublicCertificate, password)
				.GetPublicRsa();
		}

		public static System.Security.Cryptography.RSA GetPublicRsa(this PublicCertificate publicCertificate)
		{
			if (publicCertificate.Rsa == null)
			{
				publicCertificate.Rsa = publicCertificate.Cert.GetRSAPublicKey();
			}
			return publicCertificate.Rsa;
		}

		/// <summary>
		/// Finds the private key matching the passed public certificate by looking into the windows certification storage.
		/// Returns rsa-algorithm for private key.
		/// </summary>
		/// <param name="filePathToPublicCertificate"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static System.Security.Cryptography.RSA GetPrivateRsa(string filePathToPublicCertificate, string password)
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
		public static System.Security.Cryptography.RSA GetPrivateRsa(this PublicCertContentBase64String publicCertificate, string password)
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
		public static System.Security.Cryptography.RSA GetPrivateRsa(this PrivateCertContentBase64String privateCertificate)
		{
			return privateCertificate
				.GetPrivateCertificate()
				.GetPrivateRsa();
		}

		public static System.Security.Cryptography.RSA GetPrivateRsa(this PrivateCertificate privateCertificate)
		{
			if (privateCertificate.Rsa == null)
			{
				// Try the modern GetRSAPrivateKey() method first
				var rsa = privateCertificate.Cert.GetRSAPrivateKey();
				if (rsa != null)
				{
					privateCertificate.Rsa = rsa;
				}
				else
				{
					// Fallback to legacy PrivateKey property (only for compatibility)
#pragma warning disable SYSLIB0028 // PrivateKey is obsolete but needed for legacy compatibility
					if (privateCertificate.Cert.PrivateKey is System.Security.Cryptography.RSA legacyRsa)
					{
						privateCertificate.Rsa = legacyRsa;
					}
#pragma warning restore SYSLIB0028
					else
					{
						throw new InvalidOperationException("No private key found in the certificate");
					}
				}
			}
			return privateCertificate.Rsa;
		}

		#endregion

		#region private methods
		
		private static byte[] EncryptData(System.Security.Cryptography.RSA rsa, byte[] data)
		{
			// Use modern OAEP with SHA256 for all RSA implementations
			return rsa.Encrypt(data, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
		}

		private static byte[] DecryptData(System.Security.Cryptography.RSA rsa, byte[] data)
		{
			// Use modern OAEP with SHA256 for all RSA implementations
			return rsa.Decrypt(data, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subjectName">
		/// CN={subjectName}
		/// </param>
		/// <returns></returns>
		private static Certificate GetCertificateFromWindowsStorage(string subjectName)
		{
			using (var store = new X509Store(StoreLocation.LocalMachine))
			{
				try
				{
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
				catch (System.Security.Cryptography.CryptographicException)
				{
					throw; // Re-throw crypto exceptions as-is
				}
				catch (Exception ex)
				{
					throw new System.Security.Cryptography.CryptographicException(
						"Failed to access Windows certificate store.", ex);
				}
			}
		}

		public static byte[] EncryptDecrypt(this Certificate certificate, byte[] data, bool encrypt)
		{
			if (certificate == null)
				throw new ArgumentNullException(nameof(certificate));
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (data.Length == 0)
				return new byte[0];

			certificate.ThrowIfDisposed();

			try
			{
				if (certificate.Rsa == null)
					certificate.Rsa = encrypt ? ((PublicCertificate)certificate).GetPublicRsa() : ((PrivateCertificate)certificate).GetPrivateRsa();
				var rsa = certificate.Rsa;
				Func<byte[], byte[]> transform = encrypt 
					? (Func<byte[], byte[]>)(d => EncryptData(rsa, d)) 
					: d => DecryptData(rsa, d);

				var maxByteCount = rsa.KeySize / 8;
				if (encrypt)
					maxByteCount /= 2; // must be much smaller than key size

				byte[] bytesCopied = null;
				byte[] result = null;
				
				for (int i = 0; i < data.Length; i += maxByteCount)
				{
					try
					{
						if (i + maxByteCount < data.Length)
						{
							if (bytesCopied == null)
								bytesCopied = new byte[maxByteCount];
							Array.Copy(data, i, bytesCopied, 0, maxByteCount);
							var processedBytes = transform(bytesCopied);
							if (result == null)
								result = processedBytes;
							else
								result = result.ConcatBytes(processedBytes);
						}
						else
						{
							if (result == null)
								return transform(data);
							bytesCopied = new byte[data.Length - i];
							Array.Copy(data, i, bytesCopied, 0, bytesCopied.Length);
							var processedBytes = transform(bytesCopied);
							return result.ConcatBytes(processedBytes);
						}
					}
					catch (System.Security.Cryptography.CryptographicException)
					{
						throw; // Re-throw crypto exceptions as-is
					}
					catch (Exception ex)
					{
						throw new System.Security.Cryptography.CryptographicException(
							$"An error occurred during {(encrypt ? "encryption" : "decryption")} operation.", ex);
					}
				}
				return result ?? new byte[0];
			}
			catch (System.Security.Cryptography.CryptographicException)
			{
				throw; // Re-throw crypto exceptions as-is
			}
			catch (Exception ex)
			{
				throw new System.Security.Cryptography.CryptographicException(
					$"An unexpected error occurred during {(encrypt ? "encryption" : "decryption")}.", ex);
			}
		}

		#endregion
	}
}
