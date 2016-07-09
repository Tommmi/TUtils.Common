using System.Security.Cryptography.X509Certificates;
using TUtils.Common.Security.Asymmetric.Common;

namespace TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider
{
	public class PublicCertificate : Certificate, IPublicCertificate
	{
		public PublicCertificate(X509Certificate2 certificate) : base(certificate)
		{

		}

		IPublicCertContentBase64String IPublicCertificate.ToBase64String()
		{
			return this.ToBase64String();
		}

		IPrivateCertificate IPublicCertificate.GetPrivateCertificateFromWindowsStorage()
		{
			return this.GetPrivateCertificateFromWindowsStorage();
		}

		string IPublicCertificate.Encrypt(string plainText)
		{
			return this.Encrypt(plainText);
		}

		byte[] IPublicCertificate.Encrypt(byte[] plainData)
		{
			return this.Encrypt(plainData);
		}

		bool IPublicCertificate.Verify(byte[] data, byte[] signature)
		{
			return this.Verify(data, signature);
		}

		bool IPublicCertificate.Verify(byte[] data, string signature)
		{
			return this.Verify(data, signature);
		}

		bool IPublicCertificate.Verify(string text, byte[] signature)
		{
			return this.Verify(text, signature);
		}

		bool IPublicCertificate.Verify(string text, string signature)
		{
			return this.Verify(text, signature);
		}
	}
}