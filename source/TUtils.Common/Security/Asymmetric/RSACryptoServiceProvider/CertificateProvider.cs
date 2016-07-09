using TUtils.Common.Security.Asymmetric.Common;

namespace TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider
{
	// ReSharper disable once UnusedMember.Global
	public class CertificateProvider : ICertificateProvider
	{
		IPublicCertificate ICertificateProvider.GetPublicCertificateByFilePath(string filePath, string password)
		{
			return RsaLib.GetPublicCertificateByFilePath(filePath, password);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subjectName">
		/// CN={subjectName}
		/// </param>
		/// <returns></returns>
		IPublicCertificate ICertificateProvider.GetPublicCertificateFromWindowsStorage(string subjectName)
		{
			return RsaLib.GetPublicCertificateFromWindowsStorage(subjectName);
		}

		IPrivateCertificate ICertificateProvider.GetPrivateCertificateByFilePath(string filePath, string password)
		{
			return RsaLib.GetPrivateCertificateByFilePath(filePath, password);
		}

		IPrivateCertificate ICertificateProvider.GetPrivateCertificateFromWindowsStorage(string friendlyName)
		{
			return RsaLib.GetPrivateCertificateFromWindowsStorage(friendlyName);
		}

		IPublicCertContentBase64String ICertificateProvider.CreatePublicCertificateDefinitionByString(string content)
		{
			return new PublicCertContentBase64String(content);
		}

		IPrivateCertContentBase64String ICertificateProvider.CreatePrivateCertificateDefinitionByString(string content)
		{
			return new PrivateCertContentBase64String(content);
		}
	}
}