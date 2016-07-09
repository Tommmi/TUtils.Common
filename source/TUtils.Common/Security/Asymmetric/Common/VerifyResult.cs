namespace TUtils.Common.Security.Asymmetric.Common
{
	public class VerifyResult
	{
		public bool Verified { get; }
		public string ErrorText { get; }

		public VerifyResult(
			bool verified,
			string errorText)
		{
			Verified = verified;
			ErrorText = errorText;
		}
	}
}