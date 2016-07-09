using System;
using TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider;

namespace TUtils.Common.Security.Asymmetric.Common
{
	[Serializable]
	public class PublicCertContentBase64String : CertContentBase64String, IPublicCertContentBase64String
	{
		public PublicCertContentBase64String(string content) : base(content)
		{
		}

		IPublicCertificate IPublicCertContentBase64String.GetPublicCertificate(string password)
		{
			return this.GetPublicCertificate(password);
		}

		/// <summary>
		/// Looks for matching private certificate in windows certification storage
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		IPrivateCertificate IPublicCertContentBase64String.GetPrivateCertificate(string password)
		{
			return this.GetPrivateCertificate(password);
		}
	}
}