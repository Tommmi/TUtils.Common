using TUtils.Common.Security.Asymmetric.Common;

namespace TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider
{
	// ReSharper disable once UnusedMember.Global
	public class CertificateProvider : ICertificateProvider
	{
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
		IPublicCertificate ICertificateProvider.GetPublicCertificateByFilePath(string filePath, string password)
		{
			return RsaLib.GetPublicCertificateByFilePath(filePath, password);
		}

		/// <summary>
		/// finds public certification in windows certification storage by friendly name
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="subjectName">
		/// CN={subjectName}
		/// </param>
		/// <returns></returns>
		IPublicCertificate ICertificateProvider.GetPublicCertificateFromWindowsStorage(string subjectName)
		{
			return RsaLib.GetPublicCertificateFromWindowsStorage(subjectName);
		}

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
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		IPrivateCertificate ICertificateProvider.GetPrivateCertificateByFilePath(string filePath, string password)
		{
			return RsaLib.GetPrivateCertificateByFilePath(filePath, password);
		}

		/// <summary>
		/// Looks for private certificate in windows certification storage by friendly name.
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="friendlyName">
		/// The friendly name of a certificate isn't part of a certificate itself !
		/// It must be set manually in the windows certification storage on each computer.
		/// Run->Manage Computer Certificates->{select certificate}->{right click}->Properties->{set friendly name}
		/// </param>
		/// <returns></returns>
		IPrivateCertificate ICertificateProvider.GetPrivateCertificateFromWindowsStorage(string friendlyName)
		{
			return RsaLib.GetPrivateCertificateFromWindowsStorage(friendlyName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content">
		/// <code>
		/// var content = certificateProvider
		///						.GetPublicCertificateByFilePath(@"C:\tmp\cert\CER_1.cer", "starwar")
		///						.ToBase64String()
		///						.Content;
		/// </code>
		/// </param>
		/// <returns></returns>
		IPublicCertContentBase64String ICertificateProvider.CreatePublicCertificateDefinitionByString(string content)
		{
			return new PublicCertContentBase64String(content);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content">
		/// <code>
		/// string content = _publicCertificateBase64
		///							.GetPrivateCertificate("starwar")
		///							.ToBase64String()
		///							.Content;
		/// </code>
		/// </param>
		/// <returns></returns>
		IPrivateCertContentBase64String ICertificateProvider.CreatePrivateCertificateDefinitionByString(string content)
		{
			return new PrivateCertContentBase64String(content);
		}
	}
}