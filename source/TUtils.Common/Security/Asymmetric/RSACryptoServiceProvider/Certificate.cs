using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TUtils.Common.Extensions;
using TUtils.Common.Security.Asymmetric.Common;

namespace TUtils.Common.Security.Asymmetric.RSACryptoServiceProvider
{
	// ReSharper disable once RedundantExtendsListEntry
	public abstract class Certificate : ICertificate, IDisposable
	{
		public X509Certificate2 Cert { get; }

		public System.Security.Cryptography.RSACryptoServiceProvider Rsa { get; set; }

		protected Certificate(X509Certificate2 certificate)
		{
			Cert = certificate;
		}

		void IDisposable.Dispose()
		{
			Rsa?.Clear();
			Rsa = null;
		}

	}
}