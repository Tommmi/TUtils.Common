namespace TUtils.Common.Security.Asymmetric.Common
{
	public interface IPublicCertificate : ICertificate
	{
		IPublicCertContentBase64String ToBase64String();
		/// <summary>
		/// Looks for a private certificate matching the passed public certificate
		/// You must dispose returned certificate by yourself !
		/// </summary>
		/// <returns></returns>
		IPrivateCertificate GetPrivateCertificateFromWindowsStorage();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="plainText">
		/// must be UTF8 encodeable !
		/// </param>
		/// <returns>
		/// base 64 representation of the encrypted byte array of the UTF8 coded plain text byte array
		/// </returns>
		string Encrypt(string plainText);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="plainData">
		/// plain byte array 
		/// </param>
		/// <returns>
		/// encrypted byte array
		/// </returns>
		byte[] Encrypt(byte[] plainData);
		/// <summary>
		/// Verifies signature with data.
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="signature">
		/// signature of SHA1 hash from data with Pkcs1 padding
		/// </param>
		/// <returns>true if signature and data match</returns>
		bool Verify(byte[] data, byte[] signature);

		/// <summary>
		/// Verifies signature with data.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="signature">
		/// Base64 representation of signature of SHA1 hash from data
		/// </param>
		/// <returns></returns>
		bool Verify(byte[] data, string signature);


		/// <summary>
		/// Verifies signature with data.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="signature">
		/// signature of SHA1 hash of UTF8 encoded byte representation of text 
		/// </param>
		/// <returns></returns>
		bool Verify(string text, byte[] signature);

		/// <summary>
		/// Verifies signature with data.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="signature">
		/// base64 representation of signature of SHA1 hash of UTF8 encoded byte representation of text 
		/// </param>
		/// <returns></returns>
		bool Verify(string text, string signature);
	}
}