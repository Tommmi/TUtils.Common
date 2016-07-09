using TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider;

namespace TUtils.Common.Security.Asymmetric.Common
{
	public class PrivateCertContentBase64String : CertContentBase64String, IPrivateCertContentBase64String
	{
		public PrivateCertContentBase64String(string content) : base(content)
		{
		}

		IPrivateCertificate IPrivateCertContentBase64String.GetPrivateCertificate()
		{
			return this.GetPrivateCertificate();
		}
	}
}