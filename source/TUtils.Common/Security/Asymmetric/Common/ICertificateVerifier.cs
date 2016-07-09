using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUtils.Common.Security.Asymmetric.Common
{
	public interface ICertificateVerifier
	{
		VerifyResult IsValidAndTrusted(ICertificate certificate);
	}
}
