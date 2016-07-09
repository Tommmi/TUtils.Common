using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TUtils.Common.Extensions;
using TUtils.Common.Security.Asymmetric.Common;

namespace TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider
{
	public class CertificateVerifier : ICertificateVerifier
	{
		private readonly ISystemTimeProvider _timeProvider;

		public CertificateVerifier(ISystemTimeProvider timeProvider)
		{
			_timeProvider = timeProvider;
		}

		VerifyResult ICertificateVerifier.IsValidAndTrusted(ICertificate certificate)
		{
			var cert = (certificate as Certificate)?.Cert;
			if ( cert == null )
				throw new NotImplementedException("8723rhfdltr903j");

			var verified = false;
			var text = new StringBuilder();
			var chain = new X509Chain
			{
				ChainPolicy = new X509ChainPolicy()
				{
					RevocationMode = X509RevocationMode.NoCheck,
					VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid
				}
			};

			try
			{
				verified = chain.Build(cert);

				if (!verified)
					foreach (var chainStatus in chain.ChainStatus)
						text.AppendLine($"Chain error: {chainStatus.Status} {chainStatus.StatusInformation}");
			}
			catch (Exception ex)
			{
				text.AppendLine(ex.DumpException());
			}

			return new VerifyResult(
				verified: verified && (_timeProvider.LocalTime < cert.NotAfter),
				errorText: text.ToString());
		}
	}
}