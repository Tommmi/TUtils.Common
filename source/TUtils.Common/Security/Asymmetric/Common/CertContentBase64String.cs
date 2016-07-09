using System;

namespace TUtils.Common.Security.Asymmetric.Common
{
	[Serializable]
	public abstract class CertContentBase64String : ICertContentBase64String
	{
		public string Content { get; }

		public CertContentBase64String(string content)
		{
			Content = content;
		}
	}
}