namespace TUtils.Common.Security.Asymmetric.Common
{
	public interface IPublicCertContentBase64String : ICertContentBase64String
	{
		/// <summary>
		/// Gets public certificate
		/// You must dispose returned certificate by yourself !
		/// 
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		IPublicCertificate GetPublicCertificate(string password);
		/// <summary>
		/// Looks for matching private certificate in windows certification storage
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <param name="password"></param>
		/// <returns>may be null</returns>
		IPrivateCertificate GetPrivateCertificate(string password);

	}
}