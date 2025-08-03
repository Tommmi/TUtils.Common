namespace TUtils.Common.Security.Asymmetric.Common
{
	public interface IPrivateCertContentBase64String : ICertContentBase64String
	{
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
		/// <returns></returns>
		IPrivateCertificate GetPrivateCertificate();
		
		/// <summary>
		/// gets private certificate by base64 string representation with password.
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="password">Password for the private certificate</param>
		/// <returns></returns>
		IPrivateCertificate GetPrivateCertificate(string password);
	}
}