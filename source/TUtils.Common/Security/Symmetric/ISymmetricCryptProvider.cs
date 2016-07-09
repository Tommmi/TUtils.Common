using TUtils.Common.Security.Symmetric.Common;

namespace TUtils.Common.Security.Symmetric
{
	public interface ISymmetricCryptProvider
	{
		ISymmetricCrypt Create();
		ISymmetricCrypt Create(ISymmetricSecret secret);
	}
}
