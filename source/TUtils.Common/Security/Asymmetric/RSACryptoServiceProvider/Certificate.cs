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
		private bool _disposed = false;

		public X509Certificate2 Cert { get; private set; }

		public System.Security.Cryptography.RSA Rsa { get; set; }

		protected Certificate(X509Certificate2 certificate)
		{
			Cert = certificate ?? throw new ArgumentNullException(nameof(certificate));
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Dispose managed resources
					try
					{
						if (Rsa is System.Security.Cryptography.RSACryptoServiceProvider rsaCsp)
						{
							rsaCsp.Clear();
						}
						Rsa?.Dispose();
					}
					catch
					{
						// Suppress exceptions during disposal
					}
					finally
					{
						Rsa = null;
					}

					try
					{
						Cert?.Dispose();
					}
					catch
					{
						// Suppress exceptions during disposal
					}
					finally
					{
						Cert = null;
					}
				}
				_disposed = true;
			}
		}

		public void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		~Certificate()
		{
			Dispose(false);
		}
	}
}