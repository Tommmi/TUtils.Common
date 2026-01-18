namespace TUtils.Common.Security.Asymmetric.Common
{
	public interface ICertificateVerifier
	{
		VerifyResult IsValidAndTrusted(ICertificate certificate);
	}
}
