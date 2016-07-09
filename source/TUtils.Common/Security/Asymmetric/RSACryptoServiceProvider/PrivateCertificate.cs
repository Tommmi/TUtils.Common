using System.Security.Cryptography.X509Certificates;
using TUtils.Common.Security.Asymmetric.Common;

namespace TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider
{
	public class PrivateCertificate : Certificate, IPrivateCertificate
	{
		public PrivateCertificate(X509Certificate2 certificate) : base(certificate)
		{

		}

		string IPrivateCertificate.Decrypt(string encryptedBase64Text)
		{
			return this.Decrypt(encryptedBase64Text);
		}

		byte[] IPrivateCertificate.Decrypt(byte[] encryptedData)
		{
			return this.Decrypt(encryptedData);
		}

		string IPrivateCertificate.Encrypt(string plainText)
		{
			return this.Encrypt(plainText);
		}

		byte[] IPrivateCertificate.Encrypt(byte[] plainData)
		{
			return this.Encrypt(plainData);
		}

		byte[] IPrivateCertificate.Sign(byte[] data)
		{
			return this.Sign(data);
		}

		string IPrivateCertificate.SignBase64(byte[] data)
		{
			return this.SignBase64(data);
		}

		byte[] IPrivateCertificate.Sign(string text)
		{
			return this.Sign(text);
		}

		string IPrivateCertificate.SignBase64(string text)
		{
			return this.SignBase64(text);
		}

		IPrivateCertContentBase64String IPrivateCertificate.ToBase64String()
		{
			return this.ToBase64String();
		}

		IPublicCertificate IPrivateCertificate.ToPublicCertificate()
		{
			return this.ToPublicCertificate();
		}
	}
}