using TUtils.Common.Security.Symmetric.Common;

namespace TUtils.Common.Security.Symmetric.AesCryptoServiceProvider
{
	public class SymmetricCryptProvider : ISymmetricCryptProvider
	{
		ISymmetricCrypt ISymmetricCryptProvider.Create()
		{
			return new SymmetricCrypt();
		}

		ISymmetricCrypt ISymmetricCryptProvider.Create(ISymmetricSecret secret)
		{
			return new SymmetricCrypt((SymmetricSecret)secret);
		}
	}
}